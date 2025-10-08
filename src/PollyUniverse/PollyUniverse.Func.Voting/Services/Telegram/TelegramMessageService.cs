using TL;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services.Telegram;

public interface ITelegramMessageService
{
    Task<bool> SendMessage(Client telegramClient, InputPeer inputPeer, string text);

    Task<MessageBase[]> GetMessages(Client telegramClient, InputPeer inputPeer, int limit);
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

    public async Task<MessageBase[]> GetMessages(Client telegramClient, InputPeer inputPeer, int limit)
    {
        var history = await telegramClient.Messages_GetHistory(inputPeer, limit: limit);
        return history.Messages;
    }
}
