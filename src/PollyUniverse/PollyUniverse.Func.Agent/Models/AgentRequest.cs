using System.Text.Json.Serialization;
using PollyUniverse.Shared.Telegram.Converters;
using PollyUniverse.Shared.Telegram.Models;

namespace PollyUniverse.Func.Agent.Models;

public record AgentRequest
{
    [JsonPropertyName("message")]
    public AgentRequestMessage Message { get; init; }
}

public record AgentRequestMessage
{
    [JsonPropertyName("date")]
    public long Date { get; init; }

    [JsonPropertyName("text")]
    public string Text { get; init; }

    [JsonPropertyName("from")]
    public AgentRequestMessageFrom From { get; init; }

    [JsonPropertyName("chat")]
    public AgentRequestMessageChat Chat { get; init; }
}

public record AgentRequestMessageFrom
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; init; }

    [JsonPropertyName("last_name")]
    public string LastName { get; init; }

    [JsonPropertyName("username")]
    public string Username { get; init; }
}

public record AgentRequestMessageChat
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(TelegramLongPeerIdJsonConverter))]
    public TelegramLongPeerId Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; init; }
}
