using System.Text;
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
            "password" => ReadPassword(),
            _ => null
        };
    }

    private static string ReadPassword()
    {
        Console.Write("Enter password: ");

        var password = new StringBuilder();
        var completed = false;

        while (!completed)
        {
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    completed = true;
                    Console.WriteLine();
                    break;

                case ConsoleKey.Backspace:
                    if (password.Length > 0)
                    {
                        password.Remove(password.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    break;

                case var _ when key.KeyChar >= 32 && key.KeyChar <= 126:
                    password.Append(key.KeyChar);
                    Console.Write("*");
                    break;
            }
        }

        return password.ToString();
    }
}
