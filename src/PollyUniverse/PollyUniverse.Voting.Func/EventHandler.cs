using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;
using PollyUniverse.Voting.Func.Services.Files;
using PollyUniverse.Voting.Func.Services.Telegram;

namespace PollyUniverse.Voting.Func;

public interface IEventHandler
{
    Task Handle(VotingRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly ISystemsManagementService _systemsManagementService;

    private readonly ISessionFileService _sessionFileService;
    private readonly IPromptFileService _promptFileService;

    private readonly ITelegramClientService _telegramClientService;
    private readonly ITelegramMessageService _telegramMessageService;
    private readonly ITelegramPeerService _telegramPeerService;
    private readonly ITelegramPollService _telegramPollService;
    private readonly ITelegramVoteService _telegramVoteService;

    private readonly IMessageCompositionService _messageCompositionService;

    private readonly ISessionMetadataRepository _sessionMetadataRepository;
    private readonly IVotingProfileRepository _votingProfileRepository;

    private readonly ILogger<EventHandler> _logger;
    private readonly FunctionConfig _config;

    public EventHandler(
        ISystemsManagementService systemsManagementService,
        ISessionFileService sessionFileService,
        IPromptFileService promptFileService,
        ITelegramClientService telegramClientService,
        ITelegramMessageService telegramMessageService,
        ITelegramPeerService telegramPeerService,
        ITelegramPollService telegramPollService,
        ITelegramVoteService telegramVoteService,
        IMessageCompositionService messageCompositionService,
        ISessionMetadataRepository sessionMetadataRepository,
        IVotingProfileRepository votingProfileRepository,
        ILogger<EventHandler> logger,
        FunctionConfig config)
    {
        _systemsManagementService = systemsManagementService;

        _sessionFileService = sessionFileService;
        _promptFileService = promptFileService;

        _telegramClientService = telegramClientService;
        _telegramMessageService = telegramMessageService;
        _telegramPeerService = telegramPeerService;
        _telegramPollService = telegramPollService;
        _telegramVoteService = telegramVoteService;

        _messageCompositionService = messageCompositionService;

        _sessionMetadataRepository = sessionMetadataRepository;
        _votingProfileRepository = votingProfileRepository;

        _logger = logger;
        _config = config;
    }

    public async Task Handle(VotingRequest request)
    {
        _logger.LogInformation(
            "Handling voting request for SessionId: {SessionId}, VotingProfileId: {VotingProfileId}",
            request.SessionId,
            request.VotingProfileId);

        var sessionFileTask = _sessionFileService.DownloadSessionFile(request.SessionId);
        var sessionMetadataTask = _sessionMetadataRepository.Get(request.SessionId);
        var votingProfileTask = _votingProfileRepository.Get(request.VotingProfileId);

        await Task.WhenAll(sessionFileTask, sessionMetadataTask, votingProfileTask);

        var sessionFilePath = sessionFileTask.Result;
        var sessionMetadata = sessionMetadataTask.Result;
        var votingProfile = votingProfileTask.Result;

        if (sessionFilePath == null)
        {
            throw new Exception($"Failed to download session file: {request.SessionId}");
        }

        if (sessionMetadata == null)
        {
            throw new Exception($"No session metadata found: {request.SessionId}");
        }

        if (votingProfile == null)
        {
            throw new Exception($"No voting profile found: {request.VotingProfileId}");
        }

        if (request.SessionId != votingProfile.SessionId)
        {
            throw new Exception($"Voting profile session ID does not match request session ID: {request.VotingProfileId}");
        }

        _logger.LogInformation("Initializing Telegram client for SessionId: {SessionId}", request.SessionId);

        var telegramClient = await _telegramClientService.InitializeClient(sessionFilePath, sessionMetadata);

        _logger.LogInformation("Logged in successfully as {User}", telegramClient.User);
        _logger.LogInformation("Waiting for poll message for SessionId: {SessionId}", request.SessionId);

        var votingInputPeer = await _telegramPeerService.GetInputPeer(telegramClient, votingProfile.Poll.PeerId);

        if (votingInputPeer == null)
        {
            throw new Exception($"No input peer found for voting: {votingProfile.Poll.PeerId}");
        }

        var pollMessage = await _telegramPollService.WaitForPollMessage(
            telegramClient,
            votingProfile.Poll,
            TimeSpan.FromMinutes(_config.PollWaitingMinutes));

        if (pollMessage == null)
        {
            throw new Exception("No poll message received within the waiting period");
        }

        _logger.LogInformation("Received poll message with MessageId: {MessageId}", pollMessage.MessageId);

        var voted = await _telegramVoteService.Vote(telegramClient, votingInputPeer, pollMessage, votingProfile.Vote);

        if (!voted)
        {
            throw new Exception($"Failed to vote on poll message: {pollMessage.MessageId}");
        }

        var promptFileTask = _promptFileService.DownloadPromptFile("success");
        var ssmParametersTask = _systemsManagementService.GetParameters(
            _config.OpenAIApiKeyParameter,
            _config.BotTokenParameter,
            _config.NotificationsPeerIdParameter);

        await Task.WhenAll(promptFileTask, ssmParametersTask);

        var promptFilePath = promptFileTask.Result;
        var ssmParameters = ssmParametersTask.Result;
        var openAiApiKey = ssmParameters[_config.OpenAIApiKeyParameter];
        var botToken = ssmParameters[_config.BotTokenParameter];
        var notificationsPeerId = ssmParameters[_config.NotificationsPeerIdParameter];

        if (promptFilePath == null)
        {
            throw new Exception("Failed to download prompt file: success");
        }

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

        _logger.LogInformation("Composing message for SessionId: {SessionId}", request.SessionId);

        var notificationsInputPeer = await _telegramPeerService.GetInputPeer(telegramClient, long.Parse(notificationsPeerId));

        if (notificationsInputPeer == null)
        {
            throw new Exception($"No input peer found for notifications: {notificationsPeerId}");
        }

        var message = await _messageCompositionService.ComposeMessage(
            openAiApiKey,
            _config.OpenAIModel,
            promptFilePath,
            null);

        _logger.LogInformation("Sending notification message for SessionId: {SessionId}", request.SessionId);

        var messageSent = await _telegramMessageService.SendMessage(telegramClient, notificationsInputPeer, message);

        if (!messageSent)
        {
            throw new Exception("Failed to send notification message");
        }
    }
}
