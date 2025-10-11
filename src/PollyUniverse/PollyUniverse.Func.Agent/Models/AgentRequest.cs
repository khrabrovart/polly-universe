using System.Text.Json.Serialization;
using PollyUniverse.Shared.Telegram.Models;

namespace PollyUniverse.Func.Agent.Models;

public class AgentRequest
{
    [JsonPropertyName("message")]
    public AgentRequestMessage Message { get; set; }
}

public class AgentRequestMessage
{
    [JsonPropertyName("date")]
    public long Date { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("from")]
    public AgentRequestMessageFrom From { get; set; }

    [JsonPropertyName("chat")]
    public AgentRequestMessageChat Chat { get; set; }
}

public class AgentRequestMessageFrom
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; }

    [JsonPropertyName("last_name")]
    public string LastName { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; }
}

public class AgentRequestMessageChat
{
    [JsonPropertyName("id")]
    public TelegramLongPeerId Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }
}
