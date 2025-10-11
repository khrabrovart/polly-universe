using PollyUniverse.Shared.Telegram.Models;
using TL;

namespace PollyUniverse.Shared.Telegram.Extensions;

public static class InputPeerExtensions
{
    public static LongTelegramPeerId GetLongPeerId(this InputPeer inputPeer)
    {
        return inputPeer switch
        {
            InputPeerUser user => user.user_id,
            InputPeerChat chat => -chat.chat_id,
            InputPeerChannel channel => long.Parse("-100" + channel.channel_id),
            _ => throw new Exception("Unknown peer type")
        };
    }

    public static ShortTelegramPeerId GetShortPeerId(this InputPeer inputPeer)
    {
        return inputPeer.ID;
    }
}
