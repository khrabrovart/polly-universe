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
public partial class LambdaRequestJsonContext : JsonSerializerContext { }

public class Function
{
    private static readonly ServiceProvider ServiceProvider;

    static Function()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton(new FunctionConfig
        {
            SessionMetadataTable = configuration["SESSION_METADATA_TABLE"],
            VotingProfilesTable = configuration["VOTING_PROFILES_TABLE"],
            S3Bucket = configuration["S3_BUCKET"],
            IsDev = configuration["IS_DEV"] == "true"
        });

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddFilter("Amazon", Microsoft.Extensions.Logging.LogLevel.Warning);
            builder.AddFilter("AWSSDK", Microsoft.Extensions.Logging.LogLevel.Warning);
        });

        services.AddSharedServices();

        services
            .AddTransient<IEventHandler, EventHandler>()
            .AddTransient<ITelegramService, TelegramService>()
            .AddTransient<ISessionMetadataRepository, SessionMetadataRepository>()
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

            logger.LogInformation("Processing request for ProfileId: {ProfileId}", request.ProfileId);

            await handler.Handle(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing event");
        }
    }
}
