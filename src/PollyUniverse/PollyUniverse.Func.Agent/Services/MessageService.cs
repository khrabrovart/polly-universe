using PollyUniverse.Func.Agent.Models;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;
using PollyUniverse.Shared.OpenAI.Services;
using PollyUniverse.Shared.Services;
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
    private readonly IFunctionConfig _config;

    private static readonly string[] PromptIds =
    [
        "shared/system_base",
        "agent/prompt_reply",
        "shared/system_format"
    ];

    public MessageService(
        IMessageHistoryService messageHistoryService,
        ITelegramBotService telegramBotService,
        IPromptService promptService,
        IOpenAIService openAiService,
        ISystemsManagementService systemsManagementService,
        IFunctionConfig config)
    {
        _messageHistoryService = messageHistoryService;
        _telegramBotService = telegramBotService;
        _promptService = promptService;
        _openAiService = openAiService;
        _systemsManagementService = systemsManagementService;
        _config = config;
    }

    public async Task Reply(AgentRequestMessage message)
    {
        var peerId = message.Chat.Id;

        var messageHistory = await _messageHistoryService.GetHistory(peerId.ToShortPeerId());

        var lastMessage = new MessageHistoryRecord
        {
            Date = DateTimeOffset.FromUnixTimeSeconds(message.Date).DateTime,
            SenderId = message.From.Id,
            SenderFirstName = message.From.FirstName,
            SenderLastName = message.From.LastName,
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

        var generatedMessage = await _openAiService.CompleteChat(openAiApiKey, _config.OpenAIModel, prompt);

        var messageSent = await _telegramBotService.SendMessage(
            botToken,
            peerId,
            generatedMessage);

        if (!messageSent)
        {
            throw new Exception("Failed to send notification message");
        }

        var replyMessage = new MessageHistoryRecord
        {
            Date = DateTime.UtcNow,
            SenderFirstName = "Polly",
            Text = generatedMessage
        };

        messageHistory.AddMessage(lastMessage);
        messageHistory.AddMessage(replyMessage);

        await _messageHistoryService.SaveHistory(messageHistory);
    }
}
