using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Scheduling.Comparers;
using PollyUniverse.Func.Scheduling.Services;
using PollyUniverse.Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Func.Scheduling;

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

                .AddSingleton<IVotingProfileComparer, VotingProfileComparer>()

                .AddSingleton<IVotingScheduleService, VotingScheduleService>()
                ;
        });
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
