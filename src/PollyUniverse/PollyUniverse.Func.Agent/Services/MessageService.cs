using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Agent.Models;
using PollyUniverse.Func.Agent.Services.Tooling;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;
using PollyUniverse.Shared.OpenAI.Services;
using PollyUniverse.Shared.Services;
using PollyUniverse.Shared.Telegram.Models;
using PollyUniverse.Shared.Telegram.Services;

namespace PollyUniverse.Func.Agent.Services;

public interface IMessageService
{
    Task Reply(AgentRequestMessage message);
}

public class MessageService : IMessageService
{
    private readonly IMessageHistoryService _messageHistoryService;
    private readonly ITelegramBotService _telegramBotService;
    private readonly IPromptService _promptService;
    private readonly IOpenAIService _openAiService;
    private readonly ISystemsManagementService _systemsManagementService;
    private readonly IEnumerable<IToolingService> _toolingServices;
    private readonly ILogger<MessageService> _logger;
    private readonly IFunctionConfig _config;

    private static readonly string[] PromptIds =
    [
        "shared/system_base",
        "agent/prompt_reply",
        "shared/system_format_tools"
    ];

    public MessageService(
        IMessageHistoryService messageHistoryService,
        ITelegramBotService telegramBotService,
        IPromptService promptService,
        IOpenAIService openAiService,
        ISystemsManagementService systemsManagementService,
        IEnumerable<IToolingService> toolingServices,
        ILogger<MessageService> logger,
        IFunctionConfig config)
    {
        _messageHistoryService = messageHistoryService;
        _telegramBotService = telegramBotService;
        _promptService = promptService;
        _openAiService = openAiService;
        _systemsManagementService = systemsManagementService;
        _toolingServices = toolingServices;
        _logger = logger;
        _config = config;
    }

    public async Task Reply(AgentRequestMessage message)
    {
        var peerId = message.Chat.Id;

        var messageHistory = await _messageHistoryService.GetHistory((TelegramShortPeerId)peerId);

        var lastMessage = new MessageHistoryRecord
        {
            Date = DateTimeOffset.FromUnixTimeSeconds(message.Date).DateTime,
            Role = MessageHistoryRole.User,
            SenderId = message.From.Id,
            SenderName = message.From.FirstName,
            SenderUsername = message.From.Username,
            Text = message.Text
        };

        var promptTask = _promptService.GetPrompt(PromptIds, parameters: null);
        var ssmParametersTask = _systemsManagementService.GetParameters(
            _config.OpenAIApiKeyParameter,
            _config.BotTokenParameter);

        await Task.WhenAll(promptTask, ssmParametersTask);

        var prompt = promptTask.Result;
        var ssmParameters = ssmParametersTask.Result;
        var openAiApiKey = ssmParameters[_config.OpenAIApiKeyParameter];
        var botToken = ssmParameters[_config.BotTokenParameter];

        if (string.IsNullOrEmpty(openAiApiKey))
        {
            throw new Exception($"No OpenAI API key found in SSM Parameter Store: {_config.OpenAIApiKeyParameter}");
        }

        if (string.IsNullOrEmpty(botToken))
        {
            throw new Exception($"No bot token found in SSM Parameter Store: {_config.BotTokenParameter}");
        }

        var history = PrepareHistoryMessages(messageHistory, lastMessage, _config.HistoryLength);

        var tools = _toolingServices
            .SelectMany(ts => ts.GetTools())
            .ToArray();

        var generatedMessage = await _openAiService.CompleteChat(
            openAiApiKey,
            _config.OpenAIModel,
            prompt,
            history,
            tools);

        if (_config.DevFakeService)
        {
            _logger.LogInformation("{Message}", generatedMessage);
        }
        else
        {
            var messageSent = await _telegramBotService.SendMessage(
                botToken,
                peerId,
                generatedMessage);

            if (!messageSent)
            {
                throw new Exception("Failed to send notification message");
            }
        }

        var replyMessage = new MessageHistoryRecord
        {
            Date = DateTime.UtcNow,
            Role = MessageHistoryRole.Assistant,
            SenderId = 0,
            SenderName = "Polly",
            SenderUsername = "polly_bot",
            Text = generatedMessage
        };

        messageHistory.AddMessage(lastMessage);
        messageHistory.AddMessage(replyMessage);

        if (!_config.DevFakeService)
        {
            await _messageHistoryService.SaveHistory(messageHistory, _config.HistoryLength);
        }
    }

    private static string[] PrepareHistoryMessages(MessageHistory history, MessageHistoryRecord lastMessage, int historyLength)
    {
        return history.Messages
            .TakeLast(historyLength)
            .Select(m => $"{m.Date} {m.SenderName} ({m.SenderUsername}): {m.Text}")
            .Append($"{lastMessage.Date} {lastMessage.SenderName} ({lastMessage.SenderUsername}): {lastMessage.Text}")
            .ToArray();
    }
}
