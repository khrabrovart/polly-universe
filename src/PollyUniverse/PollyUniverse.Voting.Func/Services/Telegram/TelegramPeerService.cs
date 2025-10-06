using TL;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services.Telegram;

public interface ITelegramPeerService
{
    Task<InputPeer> GetInputPeer(Client telegramClient, long peerId);
}

public class TelegramPeerService : ITelegramPeerService
{
    public async Task<InputPeer> GetInputPeer(Client telegramClient, long peerId)
    {
        var allChats = await telegramClient.Messages_GetAllChats();
        return allChats.chats[peerId].ToInputPeer();
    }
}
