using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.Telegram.Services;

namespace PollyUniverse.Shared.Telegram.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTelegramServices(this IServiceCollection services)
    {
        services
            .AddSingleton(new FlurlClientCache().Add("Telegram", "https://api.telegram.org"))

            .AddSingleton<ITelegramBotService, TelegramBotService>()
            .AddSingleton<ITelegramClientService, TelegramClientService>()
            .AddSingleton<ITelegramMessageService, TelegramMessageService>()
            .AddSingleton<ITelegramPeerService, TelegramPeerService>()
            .AddSingleton<ITelegramVoteService, TelegramVoteService>()
            ;

        return services;
    }
}
