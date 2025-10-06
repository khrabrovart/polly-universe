using TL;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services.Telegram;

public interface ITelegramPeerService
{
    Task<InputPeer> GetInputPeer(Client telegramClient, long peerId);
}

public class TelegramPeerService : ITelegramPeerService
{
    private Messages_Chats _chats;

    public async Task<InputPeer> GetInputPeer(Client telegramClient, long peerId)
    {
        _chats ??= await telegramClient.Messages_GetAllChats();
        return _chats.chats.GetValueOrDefault(peerId);
    }
}
