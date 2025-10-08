using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Shared.Repositories;

public interface IVotingProfileRepository
{
    Task<VotingProfile> Get(string id);

    Task Put(VotingProfile votingProfile);
}

public class VotingProfileRepository : IVotingProfileRepository
{
    private const string Table = "polly-universe-voting-profiles";

    private readonly IDynamoDbService _dynamoDbService;

    public VotingProfileRepository(IDynamoDbService dynamoDbService)
    {
        _dynamoDbService = dynamoDbService;
    }

    public async Task<VotingProfile> Get(string id)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = id } }
        };

        var item = await _dynamoDbService.Get(Table, key);

        if (item == null)
        {
            return null;
        }

        return new VotingProfile
        {
            Id = item["Id"].S,
            Enabled = item["Enabled"].BOOL ?? false,
            Poll = new VotingProfilePoll
            {
                FromId = long.Parse(item["Poll"].M["FromId"].N),
                PeerId = long.Parse(item["Poll"].M["PeerId"].N),
                DayOfWeek = Enum.Parse<DayOfWeek>(item["Poll"].M["DayOfWeek"].S),
                Time = TimeSpan.Parse(item["Poll"].M["Time"].S),
                Timezone = item["Poll"].M["Timezone"].S,
            },
            Session = new VotingProfileSession
            {
                Id = item["Session"].M["Id"].S,
                Enabled = item["Session"].M["Enabled"].BOOL ?? false,
                VoteIndex = int.Parse(item["Session"].M["VoteIndex"].N)
            }
        };
    }

    public async Task Put(VotingProfile votingProfile)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = votingProfile.Id } },
            { "Enabled", new AttributeValue { BOOL = votingProfile.Enabled } },
            { "Poll", new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        { "FromId", new AttributeValue { N = votingProfile.Poll.FromId.ToString() } },
                        { "PeerId", new AttributeValue { N = votingProfile.Poll.PeerId.ToString() } },
                        { "DayOfWeek", new AttributeValue { S = votingProfile.Poll.DayOfWeek.ToString() } },
                        { "Time", new AttributeValue { S = votingProfile.Poll.Time.ToString() } },
                        { "Timezone", new AttributeValue { S = votingProfile.Poll.Timezone } }
                    }
                }
            },
            { "Session", new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        { "Id", new AttributeValue { S = votingProfile.Session.Id } },
                        { "Enabled", new AttributeValue { BOOL = votingProfile.Session.Enabled } },
                        { "VoteIndex", new AttributeValue { N = votingProfile.Session.VoteIndex.ToString() } }
                    }
                }
            }
        };

        await _dynamoDbService.Put(Table, item);
    }
}
