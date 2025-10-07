using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Services.Telegram;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services;

public interface INotificationService
{
    Task SendNotification(Client telegramClient, NotificationRequest notificationRequest);
}

public class NotificationService : INotificationService
{
    private readonly ISystemsManagementService _systemsManagementService;
    private readonly ITelegramMessageService _telegramMessageService;
    private readonly ITelegramPeerService _telegramPeerService;
    private readonly IMessageCompositionService _messageCompositionService;
    private readonly IPromptService _promptService;
    private readonly ILogger<NotificationService> _logger;
    private readonly FunctionConfig _config;

    public NotificationService(
        ISystemsManagementService systemsManagementService,
        ITelegramMessageService telegramMessageService,
        ITelegramPeerService telegramPeerService,
        IMessageCompositionService messageCompositionService,
        IPromptService promptService,
        ILogger<NotificationService> logger,
        FunctionConfig config)
    {
        _systemsManagementService = systemsManagementService;
        _telegramMessageService = telegramMessageService;
        _telegramPeerService = telegramPeerService;
        _messageCompositionService = messageCompositionService;
        _promptService = promptService;
        _logger = logger;
        _config = config;
    }

    public async Task SendNotification(Client telegramClient, NotificationRequest notificationRequest)
    {
        var promptTask = _promptService.GetFullPrompt("success");
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

        _logger.LogInformation("Composing notification message");

        var notificationsInputPeer = await _telegramPeerService.GetInputPeer(telegramClient, long.Parse(notificationsPeerId));

        if (notificationsInputPeer == null)
        {
            throw new Exception($"No input peer found for notifications: {notificationsPeerId}");
        }

        var message = await _messageCompositionService.ComposeMessage(
            openAiApiKey,
            _config.OpenAIModel,
            prompt,
            null);

        _logger.LogInformation("Sending notification message");

        var messageSent = await _telegramMessageService.SendMessage(telegramClient, notificationsInputPeer, message);

        if (!messageSent)
        {
            throw new Exception("Failed to send notification message");
        }
    }
}
