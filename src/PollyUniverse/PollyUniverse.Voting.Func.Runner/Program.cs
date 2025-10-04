using System.Text.Json;
using dotenv.net;

namespace PollyUniverse.Voting.Func.Runner;

class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load();

        var function = new Function();

        var evt = new SchedulerEvent
        {
            ProfileId = "default"
        };

        var input = JsonSerializer.Serialize(evt, SchedulerEventJsonContext.Default.SchedulerEvent);

        await function.HandleEvent(input, null);
    }


}
