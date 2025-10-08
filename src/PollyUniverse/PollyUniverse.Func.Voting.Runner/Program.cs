using dotenv.net;
using PollyUniverse.Func.Voting.Models;

namespace PollyUniverse.Func.Voting.Runner;

public class Program
{
    private const string TmpDirectory = "./tmp";

    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        if (Directory.Exists(TmpDirectory))
        {
            Directory.Delete(TmpDirectory, true);
        }

        var function = new Function();

        var request = new VotingRequest
        {
            VotingProfileId = Environment.GetEnvironmentVariable("DEV__VOTING_PROFILE_ID")
        };

        await function.Handle(request, null);
    }
}
