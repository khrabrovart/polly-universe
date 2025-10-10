using System.Collections.Concurrent;
using TL;
using WTelegram;

namespace PollyUniverse.Shared.Telegram.Services;

public interface ITelegramPeerService
{
    Task<InputPeer> GetInputPeer(Client telegramClient, long peerId);
}

public class TelegramPeerService : ITelegramPeerService
{
    private static readonly ConcurrentDictionary<long, Dictionary<long, ChatBase>> ChatsByUser = new();

    public async Task<InputPeer> GetInputPeer(Client telegramClient, long peerId)
    {
        var userId = telegramClient.User.id;

        if (!ChatsByUser.TryGetValue(userId, out var chats))
        {
            var allChats = await telegramClient.Messages_GetAllChats();

            chats = allChats.chats;
            ChatsByUser.TryAdd(userId, chats);
        }

        return chats.GetValueOrDefault(peerId);
    }
}
