namespace PollyUniverse.Shared.Telegram.Models;

public interface ITelegramPeerId
{
}

public class LongTelegramPeerId : ITelegramPeerId
{
    private long _value;

    public static implicit operator long(LongTelegramPeerId peerId) => peerId._value;

    public static implicit operator LongTelegramPeerId(long value)
    {
        return new LongTelegramPeerId
        {
            _value = value
        };
    }

    public ShortTelegramPeerId ToShortPeerId()
    {
        if (_value >= 0)
        {
            return _value;
        }

        var strValue = _value.ToString();

        if (strValue.StartsWith("-100"))
        {
            var shortStr = strValue[4..];
            return long.Parse(shortStr);
        }

        return -_value;
    }
}

public class ShortTelegramPeerId : ITelegramPeerId
{
    private long _value;

    public static implicit operator long(ShortTelegramPeerId peerId) => peerId._value;

    public static implicit operator ShortTelegramPeerId(long value)
    {
        return new ShortTelegramPeerId
        {
            _value = value
        };
    }
}
