using TL;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services.Telegram;

public interface ITelegramPeerService
{
    Task<InputPeer> GetInputPeer(Client telegramClient, long peerId);
}

public class TelegramPeerService : ITelegramPeerService
{
    private static readonly Dictionary<long, Messages_Chats> ChatsByUser = new();

    public async Task<InputPeer> GetInputPeer(Client telegramClient, long peerId)
    {
        var userId = telegramClient.User.id;

        if (!ChatsByUser.TryGetValue(userId, out var chats))
        {
            chats = await telegramClient.Messages_GetAllChats();
            ChatsByUser[userId] = chats;
        }

        return chats.chats.GetValueOrDefault(peerId);
    }
}
