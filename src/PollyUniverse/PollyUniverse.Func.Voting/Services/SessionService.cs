using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Voting.Services.Files;
using PollyUniverse.Func.Voting.Services.Telegram;
using PollyUniverse.Shared.Repositories;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services;

public interface ISessionService
{
    Task<Client> InitializeTelegramClientWithSession(string sessionId);
}

public class SessionService : ISessionService
{
    private readonly ISessionFileService _sessionFileService;
    private readonly ITelegramClientService _telegramClientService;
    private readonly ISessionMetadataRepository _sessionMetadataRepository;
    private readonly ILogger<SessionService> _logger;

    public SessionService(
        ISessionFileService sessionFileService,
        ITelegramClientService telegramClientService,
        ISessionMetadataRepository sessionMetadataRepository,
        ILogger<SessionService> logger)
    {
        _sessionFileService = sessionFileService;
        _telegramClientService = telegramClientService;
        _sessionMetadataRepository = sessionMetadataRepository;
        _logger = logger;
    }

    public async Task<Client> InitializeTelegramClientWithSession(string sessionId)
    {
        var sessionFileTask = _sessionFileService.DownloadSessionFile(sessionId);
        var sessionMetadataTask = _sessionMetadataRepository.Get(sessionId);

        await Task.WhenAll(sessionFileTask, sessionMetadataTask);

        var sessionFilePath = sessionFileTask.Result;
        var sessionMetadata = sessionMetadataTask.Result;

        if (sessionFilePath == null)
        {
            throw new Exception($"Failed to download session file: {sessionId}");
        }

        if (sessionMetadata == null)
        {
            throw new Exception($"No session metadata found: {sessionId}");
        }

        _logger.LogInformation("Initializing Telegram client for SessionId: {SessionId}", sessionId);

        var client = await _telegramClientService.CreateClient(sessionFilePath, sessionMetadata);

        _logger.LogInformation("Logged in successfully as {User}", client.User);

        return client;
    }
}
