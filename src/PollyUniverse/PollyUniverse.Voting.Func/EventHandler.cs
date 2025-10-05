using Microsoft.Extensions.Logging;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;

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
    private readonly ITelegramService _telegramService;
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        ISessionService sessionService,
        ISessionMetadataRepository sessionMetadataRepository,
        IVotingProfileRepository votingProfileRepository,
        ITelegramService telegramService,
        ILogger<EventHandler> logger)
    {
        _sessionService = sessionService;
        _sessionMetadataRepository = sessionMetadataRepository;
        _votingProfileRepository = votingProfileRepository;
        _telegramService = telegramService;
        _logger = logger;
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

        _logger.LogInformation("Initializing Telegram client for SessionId: {SessionId}", votingProfile.SessionId);

        var client = await _telegramService.InitializeClient(sessionFilePath, sessionMetadata);

        _logger.LogInformation("Logged in successfully as {User}", client.User);
    }
}
