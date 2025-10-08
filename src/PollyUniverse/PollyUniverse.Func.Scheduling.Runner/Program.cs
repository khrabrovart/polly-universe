using dotenv.net;

namespace PollyUniverse.Func.Scheduling.Runner;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        var function = new Function();


        await function.Handle(null, null);
    }
}
