using Microsoft.Extensions.Logging;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;
using PollyUniverse.Voting.Func.Services.Telegram;

namespace PollyUniverse.Voting.Func;

public interface IEventHandler
{
    Task Handle(VotingRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly ISessionService _sessionService;
    private readonly ISessionMetadataRepository _sessionMetadataRepository;
    private readonly IVotingProfileRepository _votingProfileRepository;
    private readonly ITelegramClientService _telegramClientService;
    private readonly ITelegramPeerService _telegramPeerService;
    private readonly ITelegramPollService _telegramPollService;
    private readonly ITelegramVoteService _telegramVoteService;
    private readonly ILogger<EventHandler> _logger;
    private readonly FunctionConfig _config;

    public EventHandler(
        ISessionService sessionService,
        ISessionMetadataRepository sessionMetadataRepository,
        IVotingProfileRepository votingProfileRepository,
        ITelegramClientService telegramClientService,
        ITelegramPeerService telegramPeerService,
        ITelegramPollService telegramPollService,
        ITelegramVoteService telegramVoteService,
        ILogger<EventHandler> logger,
        FunctionConfig config)
    {
        _sessionService = sessionService;
        _sessionMetadataRepository = sessionMetadataRepository;
        _votingProfileRepository = votingProfileRepository;
        _telegramClientService = telegramClientService;
        _telegramPeerService = telegramPeerService;
        _telegramPollService = telegramPollService;
        _telegramVoteService = telegramVoteService;
        _logger = logger;
        _config = config;
    }

    public async Task Handle(VotingRequest request)
    {
        _logger.LogInformation(
            "Handling voting request for SessionId: {SessionId}, VotingProfileId: {VotingProfileId}",
            request.SessionId,
            request.VotingProfileId);

        var sessionFileTask = _sessionService.DownloadSessionFile(request.SessionId);
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

        var client = await _telegramClientService.InitializeClient(sessionFilePath, sessionMetadata);

        _logger.LogInformation("Logged in successfully as {User}", client.User);
        _logger.LogInformation("Waiting for poll message for SessionId: {SessionId}", request.SessionId);

        var inputPeer = await _telegramPeerService.GetInputPeer(client, votingProfile.Poll.PeerId);

        if (inputPeer == null)
        {
            throw new Exception($"No input peer found: {votingProfile.Poll.PeerId}");
        }

        var pollMessage = await _telegramPollService.WaitForPollMessage(
            client,
            votingProfile.Poll,
            TimeSpan.FromMinutes(_config.PollWaitingMinutes));

        if (pollMessage == null)
        {
            throw new Exception("No poll message received within the waiting period");
        }

        _logger.LogInformation("Received poll message with MessageId: {MessageId}", pollMessage.MessageId);

        var voted = await _telegramVoteService.Vote(client, inputPeer, pollMessage, votingProfile.Vote);

        if (!voted)
        {
            throw new Exception($"Failed to vote on poll message: {pollMessage.MessageId}");
        }
    }
}
