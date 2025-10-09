using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Aws.Extensions;
using PollyUniverse.Shared.Extensions;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Func.Agent;

public class Function
{
    private static readonly ServiceProvider ServiceProvider;

    static Function()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        var functionConfig = new FunctionConfig(configuration);

        services.AddSingleton(functionConfig);

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddFilter("Amazon", Microsoft.Extensions.Logging.LogLevel.Warning);
            builder.AddFilter("AWSSDK", Microsoft.Extensions.Logging.LogLevel.Warning);
        });

        services.AddSharedServices();
        services.AddAwsServices();

        services
            .AddSingleton<IEventHandler, EventHandler>()
            ;

        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "API Gateway request cannot be null");
        }

        logger.LogInformation("Processing API Gateway request {Method} {Path}", request.HttpMethod, request.Path);

        try
        {
            var result = await handler.Handle(request);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing API Gateway request");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new { error = "Internal server error" }),
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json"
                }
            };
        }
    }
}
