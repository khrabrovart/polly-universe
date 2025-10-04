using System.Text.Json.Serialization;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Shared;
using PollyUniverse.Voting.Func;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;

[assembly: LambdaGlobalProperties(GenerateMain = true)]
[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<LambdaRequestJsonContext>))]

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

        services.AddSingleton(() => new FunctionConfig
        {
            TelegramClientDataTable = configuration["TELEGRAM_CLIENT_DATA_TABLE"],
            VotingProfilesTable = configuration["VOTING_PROFILES_TABLE"],
            S3Bucket = configuration["S3_BUCKET"],
            IsDev = bool.TryParse(configuration["IS_DEV"], out var isDev) && isDev
        });

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

    [LambdaFunction]
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

            await handler.Handle(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing event");
        }
    }
}
