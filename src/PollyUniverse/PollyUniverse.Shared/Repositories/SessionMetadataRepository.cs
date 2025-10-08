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
    private const string Table = "polly-universe-session-metadata";

    private readonly IDynamoDbService _dynamoDbService;

    public SessionMetadataRepository(IDynamoDbService dynamoDbService)
    {
        _dynamoDbService = dynamoDbService;
    }

    public async Task<SessionMetadata> Get(string clientId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = clientId } }
        };

        var item = await _dynamoDbService.Get(Table, key);

        if (item == null)
        {
            return null;
        }

        return new SessionMetadata
        {
            Id = item["Id"].S,
            ApiHash = item["ApiHash"].S,
            PhoneNumber = item["PhoneNumber"].S
        };
    }

    public async Task Put(SessionMetadata sessionMetadata)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = sessionMetadata.Id } },
            { "ApiHash", new AttributeValue { S = sessionMetadata.ApiHash } },
            { "PhoneNumber", new AttributeValue { S = sessionMetadata.PhoneNumber } }
        };

        await _dynamoDbService.Put(Table, item);
    }
}
