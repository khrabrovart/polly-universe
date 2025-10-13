using WTelegram;

namespace PollyUniverse.Shared.Telegram.Services;

public interface ITelegramClientService
{
    Task<Client> CreateClient(string sessionFilePath, string apiHash, string phoneNumber);
}

public class TelegramClientService : ITelegramClientService
{
    public async Task<Client> CreateClient(string sessionFilePath, string apiHash, string phoneNumber)
    {
        return await CreateClientAndLogin(sessionFilePath, apiHash, phoneNumber);
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
            ? throw new Exception("Failed to initialize Telegram client, session is invalid")
            : client;
    }
}
