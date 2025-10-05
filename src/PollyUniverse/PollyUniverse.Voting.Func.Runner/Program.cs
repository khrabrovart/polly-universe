using dotenv.net;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Runner;

class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load();

        var function = new Function();

        var request = new VotingRequest
        {
            SessionId = "your_session_id",
            VotingProfileId = "your_profile_id"
        };

        await function.HandleEvent(request, null);
    }


}
