using TL;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services.Telegram;

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

        return updates.UpdateList[0] is UpdateMessagePoll;
    }
}
