using System.Text.Json;
using System.Text.Json.Serialization;
using PollyUniverse.Shared.Telegram.Models;

namespace PollyUniverse.Shared.Telegram.Converters;

public class TelegramLongPeerIdJsonConverter : JsonConverter<TelegramLongPeerId>
{
    public override TelegramLongPeerId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, TelegramLongPeerId value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}
