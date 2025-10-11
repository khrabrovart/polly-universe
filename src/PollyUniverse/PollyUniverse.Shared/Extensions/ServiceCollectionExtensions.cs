using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.Aws.Extensions;
using PollyUniverse.Shared.OpenAI.Extensions;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services;
using PollyUniverse.Shared.Services.Files;
using PollyUniverse.Shared.Telegram.Extensions;

namespace PollyUniverse.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ISharedConfig>(new SharedConfig(configuration));

        services
            .AddAwsServices()
            .AddOpenAIServices()
            .AddTelegramServices();

        services
            .AddSingleton<ISessionMetadataRepository, SessionMetadataRepository>()
            .AddSingleton<IVotingProfileRepository, VotingProfileRepository>()

            .AddSingleton<IMessageHistoryFileService, MessageHistoryFileService>()
            .AddSingleton<IPromptFileService, PromptFileService>()
            .AddSingleton<ISessionFileService, SessionFileService>()

            .AddSingleton<IMessageHistoryService, MessageHistoryService>()
            ;

        return services;
    }


}
