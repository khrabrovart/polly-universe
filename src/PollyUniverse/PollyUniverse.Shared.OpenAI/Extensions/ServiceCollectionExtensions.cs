using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using PollyUniverse.Shared.OpenAI.Services;

namespace PollyUniverse.Shared.OpenAI.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddOpenAIServices(this IServiceCollection services, string apiKey)
    {
        services
            .AddSingleton(_ => new OpenAIClient(apiKey))

            .AddSingleton<IOpenAIService, OpenAIService>()
            ;
    }
}
