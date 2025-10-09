using Amazon.Lambda.DynamoDBEvents;
using dotenv.net;
using PollyUniverse.Func.Agent;

namespace PollyUniverse.Func.Scheduling.Runner;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        var function = new Function();

        var evt = new DynamoDBEvent
        {
            Records = new List<DynamoDBEvent.DynamodbStreamRecord>
            {
                new()
                {
                    EventName = "MODIFY",
                    Dynamodb = new DynamoDBEvent.StreamRecord
                    {
                        OldImage = new Dictionary<string, DynamoDBEvent.AttributeValue>
                        {
                        },
                        NewImage = new Dictionary<string, DynamoDBEvent.AttributeValue>
                        {
                        }
                    }
                }
            }
        };

        await function.Handle(evt, null);
    }
}
