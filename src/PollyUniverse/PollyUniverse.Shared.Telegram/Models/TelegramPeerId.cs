namespace PollyUniverse.Shared.Telegram.Models;

public record TelegramLongPeerId
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

    public static explicit operator TelegramShortPeerId(TelegramLongPeerId peerId)
    {
        var value = peerId._value;

        if (value >= 0)
        {
            return value;
        }

        var strValue = value.ToString();

        if (strValue.StartsWith("-100"))
        {
            var shortStr = strValue[4..];
            return long.Parse(shortStr);
        }

        return -value;
    }

    public override string ToString()
    {
        return _value.ToString();
    }
}

public record TelegramShortPeerId
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

    public override string ToString()
    {
        return _value.ToString();
    }
}
