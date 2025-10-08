using System.Net;
using System.Text.Encodings.Web;
using Flurl.Http;
using Flurl.Http.Configuration;

namespace PollyUniverse.Shared.TelegramBot.Services;

public interface ITelegramBotService
{
    Task<bool> SendMessage(string botToken, long chatId, string message);
}

public class TelegramBotService : ITelegramBotService
{
    private readonly IFlurlClient _flurlClient;

    public TelegramBotService(IFlurlClientCache flurlClientCache)
    {
        _flurlClient = flurlClientCache.Get("Telegram");
    }

    public async Task<bool> SendMessage(string botToken, long chatId, string message)
    {
        var botSegment = $"bot{UrlEncoder.Default.Encode(botToken)}";
        var response = await _flurlClient
            .Request(botSegment, "sendMessage")
            .PostJsonAsync(new
            {
                chat_id = chatId,
                text = message,
                parse_mode = "Markdown"
            });

        return response.StatusCode == (int)HttpStatusCode.OK;
    }
}
