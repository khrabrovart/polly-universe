using System.Text.Json.Serialization;

namespace PollyUniverse.Func.Agent.Models;

public class AgentRequest
{
    [JsonPropertyName("message")]
    public AgentRequestMessage Message { get; set; }
}

public class AgentRequestMessage
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
