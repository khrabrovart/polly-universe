using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;

namespace PollyUniverse.Shared.AWS;

public interface ISsmClient
{
    Task<string> GetParameter(string name);
}

public class SsmClient : ISsmClient
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;

    public SsmClient(IAmazonSimpleSystemsManagement ssmClient)
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
