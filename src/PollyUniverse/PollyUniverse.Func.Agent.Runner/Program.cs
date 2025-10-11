using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using dotenv.net;
using PollyUniverse.Func.Agent.Models;

namespace PollyUniverse.Func.Agent.Runner;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        var function = new Function();

        var agentRequest = new AgentRequest
        {
            Message = new AgentRequestMessage
            {
                Date = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Text = "Hello, this is a test message!",
                From = new AgentRequestMessageFrom
                {
                    Id = 123456789,
                    FirstName = "John",
                    LastName = "Doe",
                    Username = "johndoe"
                },
                Chat = new AgentRequestMessageChat
                {
                    Id = -1001234567890,
                    Title = "Test Channel",
                    Type = "supergroup"
                }
            }
        };

        var request = new APIGatewayProxyRequest
        {
            HttpMethod = "POST",
            Path = "/receive",
            Body = JsonSerializer.Serialize(agentRequest)
        };

        await function.Handle(request, null);
    }
}
