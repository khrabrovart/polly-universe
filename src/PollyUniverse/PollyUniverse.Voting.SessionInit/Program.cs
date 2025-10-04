using dotenv.net;

namespace PollyUniverse.Voting.SessionInit;

class Program
{
    private const string SessionFileName = "default.session";

    public static async Task Main(string[] args)
    {
        DotEnv.Load();
        WTelegram.Helpers.Log = (_, _) => { };

        var sessionFilePath = Path.Combine(AppContext.BaseDirectory, SessionFileName);

        await using var sessionStream = File.Open(sessionFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        await using var client = new WTelegram.Client(Config, sessionStream);
        var myself = await client.LoginUserIfNeeded();

        Console.WriteLine($"Session initialized for {myself.first_name} {myself.last_name} (id {myself.id})");
        Console.WriteLine($"You can find the session file at: {sessionFilePath}");
    }

    private static string Config(string what)
    {
        return what switch
        {
            "api_id" => Environment.GetEnvironmentVariable("ApiId"),
            "api_hash" => Environment.GetEnvironmentVariable("ApiHash"),
            "phone_number" => Environment.GetEnvironmentVariable("PhoneNumber"),
            "password" => Environment.GetEnvironmentVariable("Password"),
            _ => null
        };
    }
}
