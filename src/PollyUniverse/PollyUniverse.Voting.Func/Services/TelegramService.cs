using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PollyUniverse.Shared.AWS;
using PollyUniverse.Voting.Func.Repositories;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services;

public interface ITelegramService
{
    Task<Client> InitializeClient(string clientId);
}

public class TelegramService : ITelegramService
{
    private const string SessionBucketPrefix = "sessions";
    private const string LocalSessionFilePath = "/tmp/default.session";

    private readonly ITelegramClientDataRepository _telegramClientDataRepository;
    private readonly IS3Client _s3Client;
    private readonly FunctionConfig _config;

    public TelegramService(
        ITelegramClientDataRepository telegramClientDataRepository,
        IS3Client s3Client,
        ILogger<TelegramService> logger,
        IOptions<FunctionConfig> config)
    {
        _telegramClientDataRepository = telegramClientDataRepository;
        _s3Client = s3Client;
        _config = config.Value;
    }

    public async Task<Client> InitializeClient(string clientId)
    {
        var clientData = await _telegramClientDataRepository.Get(clientId);

        if (clientData == null)
        {
            throw new Exception($"No Telegram client data found for ClientId: {clientId}");
        }

        var remoteSessionFileKey = $"{SessionBucketPrefix}/{clientId}";

        var sessionFileDownloaded = await _s3Client.Download(
            _config.SessionsBucket,
            remoteSessionFileKey,
            LocalSessionFilePath);

        if (!sessionFileDownloaded)
        {
            throw new Exception($"Failed to download session file from S3: {clientId}");
        }

        return await CreateClientAndLogin(
            LocalSessionFilePath,
            clientData.ApiId,
            clientData.ApiHash,
            clientData.PhoneNumber);
    }

    private static async Task<Client> CreateClientAndLogin(string sessionFilePath, int apiId, string apiHash, string phoneNumber)
    {
        Helpers.Log = (_, _) => { };

        await using var sessionStream = File.Open(sessionFilePath, FileMode.Open, FileAccess.Read);

        var configFunc = new Func<string, string>(what => what switch
        {
            "api_id" => apiId.ToString(),
            "api_hash" => apiHash,
            "phone_number" => phoneNumber,
            _ => null
        });

        var client = new Client(configFunc, sessionStream);

        var user = await client.LoginUserIfNeeded(reloginOnFailedResume: false);

        return user == null
            ? throw new Exception("Failed to initialize Telegram client, session is not valid")
            : client;

    }
}
