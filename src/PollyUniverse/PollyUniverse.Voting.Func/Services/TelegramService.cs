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

    private readonly ISessionMetadataRepository _sessionMetadataRepository;
    private readonly IS3Client _s3Client;
    private readonly FunctionConfig _config;
    private readonly string _localSessionFilePath = "/tmp/default.session";

    public TelegramService(
        ISessionMetadataRepository sessionMetadataRepository,
        IS3Client s3Client,
        FunctionConfig config)
    {
        _sessionMetadataRepository = sessionMetadataRepository;
        _s3Client = s3Client;
        _config = config;

        if (_config.IsDev)
        {
            _localSessionFilePath = "./tmp/default.session";
        }
    }

    public async Task<Client> InitializeClient(string clientId)
    {
        var sessionMetadata = await _sessionMetadataRepository.Get(clientId);

        if (sessionMetadata == null)
        {
            throw new Exception($"No session metadata found for ClientId: {clientId}");
        }

        var remoteSessionFileKey = $"{SessionBucketPrefix}/{clientId}.session";

        var sessionFileDownloaded = await _s3Client.Download(
            _config.S3Bucket,
            remoteSessionFileKey,
            _localSessionFilePath);

        if (!sessionFileDownloaded)
        {
            throw new Exception($"Failed to download session file from S3: {clientId}");
        }

        return await CreateClientAndLogin(
            _localSessionFilePath,
            sessionMetadata.ApiId,
            sessionMetadata.ApiHash,
            sessionMetadata.PhoneNumber);
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
