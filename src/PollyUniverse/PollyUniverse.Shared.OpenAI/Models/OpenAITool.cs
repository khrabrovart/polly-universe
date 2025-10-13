namespace PollyUniverse.Shared.OpenAI.Models;

public record OpenAITool
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Returns { get; init; }

    public required Func<Dictionary<string, string>, Task<string>> Action { get; init; }

    public OpenAIToolParameter[] Parameters { get; init; }
}

public record OpenAIToolParameter
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Type { get; init; }

    public required bool Required { get; init; }
}
