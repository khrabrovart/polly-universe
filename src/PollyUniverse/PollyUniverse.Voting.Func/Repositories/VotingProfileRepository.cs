using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Repositories;

public interface IVotingProfileRepository
{
    Task<VotingProfile> Get(string profileId);
}

public class VotingProfileRepository : IVotingProfileRepository
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly FunctionConfig _config;

    public VotingProfileRepository(
        IDynamoDbService dynamoDbService,
        FunctionConfig config)
    {
        _dynamoDbService = dynamoDbService;
        _config = config;
    }

    public async Task<VotingProfile> Get(string profileId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = profileId } }
        };

        var item = await _dynamoDbService.Get(_config.VotingProfilesTable, key);

        return item == null
            ? null
            : new VotingProfile
            {
                Id = item["Id"].S,
                SessionId = item["SessionId"].S,
                Poll = new VotingProfilePoll
                {
                    FromId = long.Parse(item["Poll"].M["FromId"].N),
                    PeerId = long.Parse(item["Poll"].M["PeerId"].N),
                    UtcTime = TimeSpan.Parse(item["Poll"].M["UtcTime"].S),
                },
                Vote = new VotingProfileVote
                {
                    Index = int.Parse(item["Vote"].M["Index"].N)
                }
            };
    }
}
