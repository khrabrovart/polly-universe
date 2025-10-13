using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Shared.Repositories;

public interface ISessionMetadataRepository
{
    Task<SessionMetadata> Get(string sessionId);

    Task<SessionMetadata[]> Get(string[] sessionIds);

    Task Put(SessionMetadata sessionMetadata);
}

public class SessionMetadataRepository : ISessionMetadataRepository
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly string _tableName;

    public SessionMetadataRepository(IDynamoDbService dynamoDbService, ISharedConfig config)
    {
        _dynamoDbService = dynamoDbService;
        _tableName = config.SessionMetadataTable;
    }

    public async Task<SessionMetadata> Get(string sessionId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = sessionId } }
        };

        var item = await _dynamoDbService.Get(_tableName, key);

        if (item == null)
        {
            return null;
        }

        return new SessionMetadata
        {
            Id = item["Id"].S,
            ApiId = int.Parse(item["ApiId"].N),
            ApiHash = item["ApiHash"].S,
            PhoneNumber = item["PhoneNumber"].S,
            User = new SessionMetadataUser
            {
                Name = item["User"].M["Name"].S,
                Gender = item["User"].M["Gender"].S
            }
        };
    }

    public async Task<SessionMetadata[]> Get(string[] sessionIds)
    {
        var keys = sessionIds
            .Select(sessionId => new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = sessionId } }
            })
            .ToArray();

        var items = await _dynamoDbService.Get(_tableName, keys);

        return items.Select(ToModel).ToArray();
    }

    public async Task Put(SessionMetadata sessionMetadata)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = sessionMetadata.Id } },
            { "ApiId", new AttributeValue { N = sessionMetadata.ApiId.ToString() } },
            { "ApiHash", new AttributeValue { S = sessionMetadata.ApiHash } },
            { "PhoneNumber", new AttributeValue { S = sessionMetadata.PhoneNumber } },
            { "User", new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        { "Name", new AttributeValue { S = sessionMetadata.User.Name } },
                        { "Gender", new AttributeValue { S = sessionMetadata.User.Gender } }
                    }
                }
            }
        };

        await _dynamoDbService.Put(_tableName, item);
    }

    private static SessionMetadata ToModel(Dictionary<string, AttributeValue> item)
    {
        return new SessionMetadata
        {
            Id = item["Id"].S,
            ApiId = int.Parse(item["ApiId"].N),
            ApiHash = item["ApiHash"].S,
            PhoneNumber = item["PhoneNumber"].S,
            User = new SessionMetadataUser
            {
                Name = item["User"].M["Name"].S,
                Gender = item["User"].M["Gender"].S
            }
        };
    }
}
