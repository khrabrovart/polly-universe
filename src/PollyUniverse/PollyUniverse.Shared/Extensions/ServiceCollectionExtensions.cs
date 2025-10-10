using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.Aws.Extensions;
using PollyUniverse.Shared.OpenAI.Extensions;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Telegram.Extensions;

namespace PollyUniverse.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services
            .AddAwsServices()
            .AddOpenAIServices()
            .AddTelegramServices();

        services
            .AddSingleton<ISessionMetadataRepository, SessionMetadataRepository>()
            .AddSingleton<IVotingProfileRepository, VotingProfileRepository>()
            ;

        return services;
    }
}
