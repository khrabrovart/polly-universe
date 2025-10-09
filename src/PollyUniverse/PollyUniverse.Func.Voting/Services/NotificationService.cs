using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Func.Voting.Services.Telegram;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.TelegramBot.Services;
using PollyUniverse.Func.Voting.Extensions;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services;

public interface INotificationService
{
    Task SendNotification(Client telegramClient, VotingResult votingResult);
}

public class NotificationService : INotificationService
{
    private readonly ISystemsManagementService _systemsManagementService;
    private readonly ITelegramPeerService _telegramPeerService;
    private readonly IMessageComposeService _messageComposeService;
    private readonly IPromptService _promptService;
    private readonly ITelegramBotService _telegramBotService;
    private readonly ILogger<NotificationService> _logger;
    private readonly FunctionConfig _config;

    private static readonly Dictionary<VotingResult, string> VotingResultPrompts = new()
    {
        { VotingResult.Success, "prompt_success" },
        { VotingResult.PollNotFound, "prompt_poll_not_found" },
        { VotingResult.VoteFailed, "prompt_vote_failed" }
    };

    public NotificationService(
        ISystemsManagementService systemsManagementService,
        ITelegramPeerService telegramPeerService,
        IMessageComposeService messageComposeService,
        IPromptService promptService,
        ITelegramBotService telegramBotService,
        ILogger<NotificationService> logger,
        FunctionConfig config)
    {
        _systemsManagementService = systemsManagementService;
        _telegramPeerService = telegramPeerService;
        _messageComposeService = messageComposeService;
        _promptService = promptService;
        _telegramBotService = telegramBotService;
        _logger = logger;
        _config = config;
    }

    public async Task SendNotification(Client telegramClient, VotingResult votingResult)
    {
        if (_config.DevMuteNotifications)
        {
            return;
        }

        var promptId = VotingResultPrompts.GetValueOrDefault(votingResult);

        if (string.IsNullOrEmpty(promptId))
        {
            throw new Exception($"No prompt ID found for voting result: {votingResult}");
        }

        var promptTask = _promptService.GetFullPrompt(promptId);
        var ssmParametersTask = _systemsManagementService.GetParameters(
            _config.OpenAIApiKeyParameter,
            _config.BotTokenParameter,
            _config.NotificationsPeerIdParameter);

        await Task.WhenAll(promptTask, ssmParametersTask);

        var prompt = promptTask.Result;
        var ssmParameters = ssmParametersTask.Result;
        var openAiApiKey = ssmParameters[_config.OpenAIApiKeyParameter];
        var botToken = ssmParameters[_config.BotTokenParameter];
        var notificationsPeerId = ssmParameters[_config.NotificationsPeerIdParameter];

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            throw new Exception($"No OpenAI API key found in SSM Parameter Store: {_config.OpenAIApiKeyParameter}");
        }

        if (string.IsNullOrEmpty(botToken))
        {
            throw new Exception($"No bot token found in SSM Parameter Store: {_config.BotTokenParameter}");
        }

        if (string.IsNullOrEmpty(notificationsPeerId))
        {
            throw new Exception($"No notifications peer ID found in SSM Parameter Store: {_config.NotificationsPeerIdParameter}");
        }

        var notificationsInputPeer = await _telegramPeerService.GetInputPeer(telegramClient, long.Parse(notificationsPeerId));

        if (notificationsInputPeer == null)
        {
            throw new Exception($"No input peer found for notifications: {notificationsPeerId}");
        }

        var user = telegramClient.User;

        var message = await _messageComposeService.ComposeMessage(
            votingResult,
            user.ID,
            user.first_name,
            openAiApiKey,
            _config.OpenAIModel,
            prompt,
            null);

        var messageSent = await _telegramBotService.SendMessage(
            botToken,
            notificationsInputPeer.GetTelegramPeerId(),
            message);

        if (!messageSent)
        {
            throw new Exception("Failed to send notification message");
        }
    }
}
