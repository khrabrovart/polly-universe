using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Agent.Models;

namespace PollyUniverse.Func.Agent;

public interface IEventHandler
{
    Task Handle(APIGatewayProxyRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        ILogger<EventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(APIGatewayProxyRequest request)
    {
        var body = JsonSerializer.Deserialize<AgentRequest>(request.Body);
    }

    private async Task<APIGatewayProxyResponse> HandlePostRequest(APIGatewayProxyRequest request)
    {
        _logger.LogInformation("Handling POST request for path: {Path}", request.Path);

        try
        {
            var requestBody = request.Body;
            _logger.LogInformation("Request body: {Body}", requestBody);

            var responseBody = new
            {
                message = "POST request processed successfully",
                receivedData = requestBody,
                timestamp = DateTime.UtcNow.ToString("O")
            };

            return new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = JsonSerializer.Serialize(responseBody),
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json",
                    ["Access-Control-Allow-Origin"] = "*"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing POST request");
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = JsonSerializer.Serialize(new { error = "Invalid request data" }),
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json",
                    ["Access-Control-Allow-Origin"] = "*"
                }
            };
        }
    }
}
