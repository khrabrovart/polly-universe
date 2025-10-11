using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Scheduling.Comparers;
using PollyUniverse.Func.Scheduling.Services;
using PollyUniverse.Shared.Extensions;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Func.Scheduling;

public class Function
{
    private static readonly ServiceProvider ServiceProvider;

    static Function()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddFilter("Amazon", Microsoft.Extensions.Logging.LogLevel.Warning);
            builder.AddFilter("AWSSDK", Microsoft.Extensions.Logging.LogLevel.Warning);
        });

        services.AddSharedServices(configuration);

        services
            .AddSingleton<IFunctionConfig>(new FunctionConfig(configuration))

            .AddSingleton<IEventHandler, EventHandler>()

            .AddSingleton<IVotingProfileComparer, VotingProfileComparer>()

            .AddSingleton<IVotingScheduleService, VotingScheduleService>()
            ;

        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task Handle(DynamoDBEvent evt, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();

        if (evt == null)
        {
            throw new ArgumentNullException(nameof(evt), "DynamoDB stream event cannot be null");
        }

        logger.LogInformation("Processing DynamoDB stream event \"{Event}\"", JsonSerializer.Serialize(evt));

        await handler.Handle(evt);
    }
}
