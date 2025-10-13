using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Shared.Repositories;

public interface IUserRepository
{
    Task<User> Get(string userId);

    Task<User[]> Get(string[] userIds);

    Task Update(User user);
}

public class UserRepository : IUserRepository
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly string _tableName;

    public UserRepository(IDynamoDbService dynamoDbService, ISharedConfig config)
    {
        _dynamoDbService = dynamoDbService;
        _tableName = config.UsersTable;
    }

    public async Task<User> Get(string userId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = userId } }
        };

        var item = await _dynamoDbService.Get(_tableName, key);

        return item == null ? null : ToModel(item);
    }

    public async Task<User[]> Get(string[] userIds)
    {
        var keys = userIds
            .Select(userId => new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = userId } }
            })
            .ToArray();

        var items = await _dynamoDbService.Get(_tableName, keys);

        return items.Select(ToModel).ToArray();
    }

    public async Task Update(User user)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = user.Id } },
            { "ApiId", new AttributeValue { N = user.ApiId.ToString() } },
            { "ApiHash", new AttributeValue { S = user.ApiHash } },
            { "PhoneNumber", new AttributeValue { S = user.PhoneNumber } },
            { "Name", new AttributeValue { S = user.Name } },
            { "Gender", new AttributeValue { S = user.Gender } }
        };

        await _dynamoDbService.Put(_tableName, item);
    }

    private static User ToModel(Dictionary<string, AttributeValue> item)
    {
        return new User
        {
            Id = item["Id"].S,
            ApiId = int.Parse(item["ApiId"].N),
            ApiHash = item["ApiHash"].S,
            PhoneNumber = item["PhoneNumber"].S,
            Name = item["Name"].S,
            Gender = item["Gender"].S
        };
    }
}
