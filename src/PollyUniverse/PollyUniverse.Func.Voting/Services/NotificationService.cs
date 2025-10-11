using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.OpenAI.Services;
using PollyUniverse.Shared.Services;
using PollyUniverse.Shared.Telegram.Extensions;
using PollyUniverse.Shared.Telegram.Services;
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
    private readonly ITelegramBotService _telegramBotService;
    private readonly IPromptService _promptService;
    private readonly IOpenAIService _openAiService;
    private readonly IFunctionConfig _config;

    private const string SystemBasePromptId = "shared/system_base";
    private const string SystemFormatPromptId = "shared/system_format";

    private static readonly Dictionary<VotingResult, string> VotingResultPrompts = new()
    {
        { VotingResult.Success, "voting/prompt_success" },
        { VotingResult.PollNotFound, "voting/prompt_poll_not_found" },
        { VotingResult.VoteFailed, "voting/prompt_vote_failed" }
    };

    private static readonly Dictionary<VotingResult, string> VotingResultMarks = new()
    {
        { VotingResult.Success, "✅"},
        { VotingResult.PollNotFound, "🔎"},
        { VotingResult.VoteFailed, "❌"}
    };

    public NotificationService(
        ISystemsManagementService systemsManagementService,
        ITelegramPeerService telegramPeerService,
        ITelegramBotService telegramBotService,
        IPromptService promptService,
        IOpenAIService openAiService,
        IFunctionConfig config)
    {
        _systemsManagementService = systemsManagementService;
        _telegramPeerService = telegramPeerService;
        _telegramBotService = telegramBotService;
        _promptService = promptService;
        _openAiService = openAiService;
        _config = config;
    }

    public async Task SendNotification(Client telegramClient, VotingResult votingResult)
    {
        if (_config.DevMuteNotifications)
        {
            return;
        }

        var promptTask = GetPrompt(votingResult);
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
        var generatedMessage =  await _openAiService.CompleteChat(openAiApiKey, _config.OpenAIModel, prompt);
        generatedMessage = PostprocessMessage(generatedMessage, user.ID, user.first_name, votingResult);

        var messageSent = await _telegramBotService.SendMessage(
            botToken,
            notificationsInputPeer.GetLongPeerId(),
            generatedMessage);

        if (!messageSent)
        {
            throw new Exception("Failed to send notification message");
        }
    }

    private Task<string> GetPrompt(VotingResult votingResult)
    {
        var promptId = VotingResultPrompts.GetValueOrDefault(votingResult);

        if (string.IsNullOrEmpty(promptId))
        {
            throw new Exception($"No prompt ID found for voting result: {votingResult}");
        }

        var orderedPrompts = new[] { SystemBasePromptId, promptId, SystemFormatPromptId };

        return _promptService.GetPrompt(orderedPrompts, parameters: null);
    }

    private static string PostprocessMessage(string message, long userId, string userName, VotingResult votingResult)
    {
        var userMention = $"[{userName}](tg://user?id={userId})";
        var mark = VotingResultMarks.GetValueOrDefault(votingResult, string.Empty);

        return $"{mark} {message}".Replace("PLAYER_NAME", userMention);
    }
}
