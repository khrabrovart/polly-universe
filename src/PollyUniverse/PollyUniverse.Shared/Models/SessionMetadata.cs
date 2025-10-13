namespace PollyUniverse.Shared.Models;

public class SessionMetadata
{
    public required string Id { get; set; }

    public required int ApiId { get; set; }

    public required string ApiHash { get; set; }

    public required string PhoneNumber { get; set; }

    public required SessionMetadataUser User { get; set; }
}

public class SessionMetadataUser
{
    public required string Name { get; set; }

    public required string Gender { get; set; }
}
