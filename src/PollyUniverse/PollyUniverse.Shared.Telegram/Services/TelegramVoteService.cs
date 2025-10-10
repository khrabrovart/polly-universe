using TL;
using WTelegram;

namespace PollyUniverse.Shared.Telegram.Services;

public interface ITelegramVoteService
{
    Task<bool> Vote(Client telegramClient, InputPeer inputPeer, int messageId, byte[] option);
}

public class TelegramVoteService : ITelegramVoteService
{
    public async Task<bool> Vote(
        Client telegramClient,
        InputPeer inputPeer,
        int messageId,
        byte[] option)
    {
        var updates = await telegramClient.Messages_SendVote(inputPeer, messageId, option);
        return updates.UpdateList[0] is UpdateMessagePoll;
    }
}
