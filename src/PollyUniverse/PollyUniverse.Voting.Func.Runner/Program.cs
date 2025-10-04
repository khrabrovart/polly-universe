using System.Text.Json;
using dotenv.net;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Runner;

class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load();

        var function = new Function();

        var evt = new LambdaRequest
        {
            ProfileId = "your_profile_id"
        };

        var input = JsonSerializer.Serialize(evt, LambdaRequestJsonContext.Default.LambdaRequest);

        await function.HandleEvent(input, null);
    }


}
