using Amazon.Lambda.APIGatewayEvents;
using dotenv.net;

namespace PollyUniverse.Func.Agent.Runner;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        var function = new Function();

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Path = "/receive",
            Body = @"{ ""votingProfileId"": ""your-voting-profile-id"" }"
        };

        await function.Handle(request, null);
    }
}
