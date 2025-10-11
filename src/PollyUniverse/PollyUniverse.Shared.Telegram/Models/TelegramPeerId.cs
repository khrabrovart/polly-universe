namespace PollyUniverse.Shared.Telegram.Models;

public interface ITelegramPeerId
{
}

public class TelegramLongPeerId : ITelegramPeerId
{
    private long _value;

    public static implicit operator long(TelegramLongPeerId peerId) => peerId._value;

    public static implicit operator TelegramLongPeerId(long value)
    {
        return new TelegramLongPeerId
        {
            _value = value
        };
    }

    public TelegramShortPeerId ToShortPeerId()
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

public class TelegramShortPeerId : ITelegramPeerId
{
    private long _value;

    public static implicit operator long(TelegramShortPeerId peerId) => peerId._value;

    public static implicit operator TelegramShortPeerId(long value)
    {
        return new TelegramShortPeerId
        {
            _value = value
        };
    }
}
