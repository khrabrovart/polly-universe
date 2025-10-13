using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services.Files;
using PollyUniverse.Shared.Telegram.Services;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services;

public interface IUserService
{
    Task<Client> InitializeTelegramClientForUser(string userId);
}

public class UserService : IUserService
{
    private readonly ISessionFileService _sessionFileService;
    private readonly ITelegramClientService _telegramClientService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
        ISessionFileService sessionFileService,
        ITelegramClientService telegramClientService,
        IUserRepository userRepository,
        ILogger<UserService> logger)
    {
        _sessionFileService = sessionFileService;
        _telegramClientService = telegramClientService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Client> InitializeTelegramClientForUser(string userId)
    {
        var sessionFileTask = _sessionFileService.DownloadSessionFile(userId);
        var userTask = _userRepository.Get(userId);

        await Task.WhenAll(sessionFileTask, userTask);

        var sessionFilePath = sessionFileTask.Result;
        var user = userTask.Result;

        if (sessionFilePath == null)
        {
            throw new Exception($"Failed to download session file: {userId}");
        }

        if (user == null)
        {
            throw new Exception($"No user found: {userId}");
        }

        _logger.LogInformation("Initializing Telegram client for user \"{UserId}\"", userId);

        var client = await _telegramClientService.CreateClient(
            sessionFilePath,
            user.ApiHash,
            user.PhoneNumber);

        _logger.LogInformation("Logged in successfully as \"{User}\"", client.User);

        return client;
    }
}
