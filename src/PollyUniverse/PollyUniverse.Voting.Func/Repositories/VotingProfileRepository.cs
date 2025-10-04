using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using PollyUniverse.Shared.AWS;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Repositories;

public interface IVotingProfileRepository
{
    Task<VotingProfile> Get(string profileId);
}

public class VotingProfileRepository : IVotingProfileRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly FunctionConfig _config;

    public VotingProfileRepository(
        IDynamoDbClient dynamoDbClient,
        IOptions<FunctionConfig> config)
    {
        _dynamoDbClient = dynamoDbClient;
        _config = config.Value;
    }

    public async Task<VotingProfile> Get(string profileId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = profileId } }
        };

        var item = await _dynamoDbClient.Get(_config.VotingProfilesTable, key);

        return item == null
            ? null
            : new VotingProfile
            {
                Id = item["Id"].S,
                TelegramClientId = item["TelegramClientId"].S
            };
    }
}
