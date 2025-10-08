using PollyUniverse.Shared.Models;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services.Telegram;

public interface ITelegramClientService
{
    Task<Client> CreateClient(string sessionFilePath, SessionMetadata sessionMetadata);
}

public class TelegramClientService : ITelegramClientService
{
    public async Task<Client> CreateClient(string sessionFilePath, SessionMetadata sessionMetadata)
    {
        return await CreateClientAndLogin(sessionFilePath, sessionMetadata.ApiHash, sessionMetadata.PhoneNumber);
    }

    private static async Task<Client> CreateClientAndLogin(string sessionFilePath, string apiHash, string phoneNumber)
    {
        Helpers.Log = (_, _) => { };

        await using var sessionStream = File.Open(sessionFilePath, FileMode.Open, FileAccess.Read);

        var configFunc = new Func<string, string>(what => what switch
        {
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
