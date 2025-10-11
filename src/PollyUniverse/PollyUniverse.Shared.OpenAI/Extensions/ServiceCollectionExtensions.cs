using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.OpenAI.Services;

namespace PollyUniverse.Shared.OpenAI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IOpenAIService, OpenAIService>()
            ;

        return services;
    }
}
