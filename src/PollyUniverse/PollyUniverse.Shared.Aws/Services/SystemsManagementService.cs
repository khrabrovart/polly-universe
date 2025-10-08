using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace PollyUniverse.Shared.Aws.Services;

public interface ISystemsManagementService
{
    Task<string> GetParameter(string name);

    Task<Dictionary<string, string>> GetParameters(params string[] parameters);
}

public class SystemsManagementService : ISystemsManagementService
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;

    public SystemsManagementService(IAmazonSimpleSystemsManagement ssmClient)
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

    public async Task<Dictionary<string, string>> GetParameters(params string[] parameters)
    {
        var request = new GetParametersRequest
        {
            Names = parameters.ToList(),
            WithDecryption = true
        };

        var response = await _ssmClient.GetParametersAsync(request);

        return response.Parameters.ToDictionary(p => p.Name, p => p.Value);
    }
}
