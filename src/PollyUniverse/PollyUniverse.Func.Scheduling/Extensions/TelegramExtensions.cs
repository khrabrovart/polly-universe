using TL;

namespace PollyUniverse.Func.Scheduling.Extensions;

public static class TelegramExtensions
{
    public static long GetTelegramPeerId(this InputPeer inputPeer)
    {
        return inputPeer switch
        {
            InputPeerUser user => user.user_id,
            InputPeerChat chat => -chat.chat_id,
            InputPeerChannel channel => long.Parse("-100" + channel.channel_id),
            _ => throw new Exception("Unknown peer type")
        };
    }
}
