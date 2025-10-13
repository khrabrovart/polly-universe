namespace PollyUniverse.Shared.Models;

public class User
{
    public required string Id { get; set; }

    public required int ApiId { get; set; }

    public required string ApiHash { get; set; }

    public required string PhoneNumber { get; set; }

    public required string Name { get; set; }

    public required string Gender { get; set; }
}
