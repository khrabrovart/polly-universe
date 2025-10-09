using dotenv.net;

namespace PollyUniverse.Tools.SessionInit;

class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        WTelegram.Helpers.Log = (_, _) => { };

        var apiId = Environment.GetEnvironmentVariable("API_ID");
        var sessionFileName = $"{apiId}.session";
        var sessionFilePath = Path.Combine(AppContext.BaseDirectory, sessionFileName);

        await using var sessionStream = File.Open(sessionFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        await using var client = new WTelegram.Client(Config, sessionStream);
        var myself = await client.LoginUserIfNeeded();

        Console.Clear();
        Console.WriteLine($"Session initialized for {myself.first_name} {myself.last_name} (id {myself.id})");
        Console.WriteLine($"You can find the session file at: {sessionFilePath}");
    }

    private static string Config(string what)
    {
        return what switch
        {
            "api_id" => Environment.GetEnvironmentVariable("API_ID"),
            "api_hash" => Environment.GetEnvironmentVariable("API_HASH"),
            "phone_number" => Environment.GetEnvironmentVariable("PHONE_NUMBER"),
            _ => null
        };
    }
}
