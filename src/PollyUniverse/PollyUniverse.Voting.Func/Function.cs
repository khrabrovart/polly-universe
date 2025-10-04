using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Shared;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Voting.Func;

[JsonSerializable(typeof(LambdaRequest))]
public partial class LambdaRequestJsonContext : JsonSerializerContext {}

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

        services.AddSharedServices();

        services
            .AddTransient<IEventHandler, EventHandler>()
            .AddTransient<ITelegramService, TelegramService>()
            .AddTransient<ITelegramClientDataRepository, TelegramClientDataRepository>()
            .AddTransient<IVotingProfileRepository, VotingProfileRepository>()
            ;

        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task HandleEvent(LambdaRequest request, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();

        try
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Lambda request cannot be null");
            }

            logger.LogInformation("Received event: {Event}", JsonSerializer.Serialize(request));

            await handler.Handle(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing event");
        }
    }
}
