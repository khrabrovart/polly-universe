using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Aws.Extensions;
using PollyUniverse.Shared.OpenAI.Extensions;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;
using PollyUniverse.Voting.Func.Services.Files;
using PollyUniverse.Voting.Func.Services.Telegram;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Voting.Func;

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

        services.AddAwsServices();
        services.AddOpenAIServices();

        services
            .AddSingleton<IEventHandler, EventHandler>()

            .AddSingleton<IPromptFileService, PromptFileService>()
            .AddSingleton<ISessionFileService, SessionFileService>()

            .AddSingleton<ITelegramClientService, TelegramClientService>()
            .AddSingleton<ITelegramMessageService, TelegramMessageService>()
            .AddSingleton<ITelegramPeerService, TelegramPeerService>()
            .AddSingleton<ITelegramPollService, TelegramPollService>()
            .AddSingleton<ITelegramVoteService, TelegramVoteService>()

            .AddSingleton<IMessageCompositionService, MessageCompositionService>()

            .AddSingleton<ISessionMetadataRepository, SessionMetadataRepository>()
            .AddSingleton<IVotingProfileRepository, VotingProfileRepository>()
            ;

        ServiceProvider = services.BuildServiceProvider();
    }

    public async Task HandleEvent(VotingRequest request, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();

        try
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Voting request cannot be null");
            }

            logger.LogInformation(
                "Processing request for SessionId: {SessionId}, VotingProfileId: {ProfileId}",
                request.SessionId,
                request.VotingProfileId);

            await handler.Handle(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while processing voting request");
        }
    }
}
