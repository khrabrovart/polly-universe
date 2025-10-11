using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Shared.Repositories;

public interface ISessionMetadataRepository
{
    Task<SessionMetadata> Get(string clientId);

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

    public async Task<SessionMetadata> Get(string clientId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = clientId } }
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
            PhoneNumber = item["PhoneNumber"].S
        };
    }

    public async Task Put(SessionMetadata sessionMetadata)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = sessionMetadata.Id } },
            { "ApiId", new AttributeValue { N = sessionMetadata.ApiId.ToString() } },
            { "ApiHash", new AttributeValue { S = sessionMetadata.ApiHash } },
            { "PhoneNumber", new AttributeValue { S = sessionMetadata.PhoneNumber } }
        };

        await _dynamoDbService.Put(_tableName, item);
    }
}
