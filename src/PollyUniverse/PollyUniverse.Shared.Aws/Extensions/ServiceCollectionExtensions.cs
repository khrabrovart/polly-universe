using Amazon.DynamoDBv2;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.Aws.Services;

namespace PollyUniverse.Shared.Aws.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddAwsServices(this IServiceCollection services)
    {
        services
            .AddAWSService<IAmazonDynamoDB>()
            .AddAWSService<IAmazonS3>()
            .AddAWSService<IAmazonSimpleSystemsManagement>()

            .AddSingleton<IS3Service, S3Service>()
            .AddSingleton<IDynamoDbService, DynamoDbService>()
            .AddSingleton<ISystemsManagementService, SystemsManagementService>()
            ;
    }
}
