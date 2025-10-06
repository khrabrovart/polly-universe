using dotenv.net;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Runner;

class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: ["dev.env"]));

        var function = new Function();

        var request = new VotingRequest
        {
            SessionId = Environment.GetEnvironmentVariable("DEV__SESSION_ID")!,
            VotingProfileId = Environment.GetEnvironmentVariable("DEV__VOTING_PROFILE_ID")!
        };

        await function.HandleEvent(request, null);
    }


}
