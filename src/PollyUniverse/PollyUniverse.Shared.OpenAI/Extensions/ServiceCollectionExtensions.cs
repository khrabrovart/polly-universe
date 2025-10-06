using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.OpenAI.Services;

namespace PollyUniverse.Shared.OpenAI.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddOpenAIServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IOpenAIService, OpenAIService>()
            ;
    }
}
