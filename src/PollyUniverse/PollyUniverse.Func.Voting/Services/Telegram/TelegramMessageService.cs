using TL;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services.Telegram;

public interface ITelegramMessageService
{
    Task<bool> SendMessage(Client telegramClient, InputPeer inputPeer, string text);
}

public class TelegramMessageService : ITelegramMessageService
{
    public async Task<bool> SendMessage(Client telegramClient, InputPeer inputPeer, string text)
    {
        var updates = await telegramClient.Messages_SendMessage(
            inputPeer,
            text,
            Random.Shared.NextInt64());

        return updates.UpdateList[0] is UpdateNewMessage;
    }
}
