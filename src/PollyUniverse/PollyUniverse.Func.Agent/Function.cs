using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Agent.Services;
using PollyUniverse.Func.Agent.Services.Tooling;
using PollyUniverse.Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Func.Agent;

public class Function
{
    private static readonly ServiceProvider ServiceProvider;

    static Function()
    {
        ServiceProvider = SharedBootstrapper.Bootstrap((services, configuration) =>
        {
            services
                .AddSingleton<IFunctionConfig>(new FunctionConfig(configuration))

                .AddSingleton<IEventHandler, EventHandler>()

                .AddSingleton<IMessageService, MessageService>()

                .AddSingleton<IToolingService, VotingProfileService>()
                ;
        });
    }

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();
        var config = ServiceProvider.GetRequiredService<IFunctionConfig>();

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "API Gateway request cannot be null");
        }

        logger.LogInformation("Processing API Gateway request {Method} {Path} \"{Body}\"", request.HttpMethod, request.Path, request.Body);

        try
        {
            await handler.Handle(request);
        }
        catch
        {
            if (config.DevFakeService)
            {
                throw;
            }

            // Ignore any exceptions to avoid Telegram Bot webhook failure
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = 200,
            Body = JsonSerializer.Serialize(new { message = "Success" }),
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            }
        };
    }
}
