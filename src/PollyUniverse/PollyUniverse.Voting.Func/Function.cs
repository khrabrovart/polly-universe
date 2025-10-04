using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Core;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Voting.Func;

[JsonSerializable(typeof(SchedulerEvent))]
public partial class SchedulerEventJsonContext : JsonSerializerContext {}

public class Function
{
    private static readonly ServiceProvider ServiceProvider;

    static Function()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.Configure<FunctionConfig>(configuration.GetSection("Voting"));

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        services.AddDefaultAWSOptions(configuration.GetAWSOptions());

        services
            .AddAWSService<IAmazonDynamoDB>()
            .AddAWSService<IAmazonS3>()
            .AddTransient<IEventHandler, EventHandler>()
            ;

        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task HandleEvent(string input, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();

        try
        {
            var evt = JsonSerializer.Deserialize(input, SchedulerEventJsonContext.Default.SchedulerEvent);

            if (evt == null)
            {
                throw new JsonException("Failed to deserialize event object");
            }

            logger.LogInformation("Received event: {Event}", JsonSerializer.Serialize(evt));

            await handler.Handle(evt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing event");
        }
    }
}
