using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Shared.Repositories;

public interface IVotingProfileRepository
{
    Task<VotingProfile[]> GetAll();

    Task<VotingProfile> Get(string id);

    Task Put(VotingProfile votingProfile);
}

public class VotingProfileRepository : IVotingProfileRepository
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly string _tableName;

    public VotingProfileRepository(IDynamoDbService dynamoDbService, ISharedConfig config)
    {
        _dynamoDbService = dynamoDbService;
        _tableName = config.VotingProfilesTable;
    }

    public async Task<VotingProfile[]> GetAll()
    {
        var items = await _dynamoDbService.Scan(_tableName);

        return items.Select(ToModel).ToArray();
    }

    public async Task<VotingProfile> Get(string id)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = id } }
        };

        var item = await _dynamoDbService.Get(_tableName, key);

        return item == null ? null : ToModel(item);
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
            { "Sessions", new AttributeValue
                {
                    L = votingProfile.Sessions.Select(session => new AttributeValue
                    {
                        M = new Dictionary<string, AttributeValue>
                        {
                            { "Id", new AttributeValue { S = session.Id } },
                            { "Enabled", new AttributeValue { BOOL = session.Enabled } },
                            { "VoteIndex", new AttributeValue { N = session.VoteIndex.ToString() } },
                            { "VoteDelaySeconds", new AttributeValue { N = session.VoteDelaySeconds.ToString() } }
                        }
                    }).ToList()
                }
            }
        };

        await _dynamoDbService.Put(_tableName, item);
    }

    private static VotingProfile ToModel(Dictionary<string, AttributeValue> item)
    {
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
            Sessions = item["Sessions"].L
                .Select(session => new VotingProfileSession
                {
                    Id = session.M["Id"].S,
                    Enabled = session.M["Enabled"].BOOL ?? false,
                    VoteIndex = int.Parse(session.M["VoteIndex"].N),
                    VoteDelaySeconds = int.Parse(session.M["VoteDelaySeconds"].N)
                })
                .ToList()
        };
    }
}
