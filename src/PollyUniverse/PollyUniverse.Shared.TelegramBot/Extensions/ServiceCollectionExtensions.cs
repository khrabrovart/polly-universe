using Flurl.Http.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.TelegramBot.Services;

namespace PollyUniverse.Shared.TelegramBot.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTelegramBotServices(this IServiceCollection services)
    {
        services
            .AddSingleton(new FlurlClientCache().Add("Telegram", "https://api.telegram.org"))

            .AddSingleton<ITelegramBotService, TelegramBotService>()
            ;
    }
}
