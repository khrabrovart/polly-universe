using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.Repositories;

namespace PollyUniverse.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services
            .AddSingleton<ISessionMetadataRepository, SessionMetadataRepository>()
            .AddSingleton<IVotingProfileRepository, VotingProfileRepository>()
            ;
    }
}
