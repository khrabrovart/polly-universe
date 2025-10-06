using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace PollyUniverse.Shared.Aws.Services;

public interface ISystemManagementService
{
    Task<string> GetParameter(string name);
}

public class SystemManagementService : ISystemManagementService
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;

    public SystemManagementService(IAmazonSimpleSystemsManagement ssmClient)
    {
        _ssmClient = ssmClient;
    }

    public async Task<string> GetParameter(string name)
    {
        var request = new GetParameterRequest
        {
            Name = name,
            WithDecryption = true
        };

        var response = await _ssmClient.GetParameterAsync(request);

        return response.Parameter.Value;
    }
}
