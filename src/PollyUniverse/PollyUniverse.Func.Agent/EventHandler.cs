using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;

namespace PollyUniverse.Func.Agent;

public interface IEventHandler
{
    Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        ILogger<EventHandler> logger)
    {
        _logger = logger;
    }

    public async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest request)
    {
        _logger.LogInformation("Processing API Gateway request: {Method} {Path}",
            request.HttpMethod, request.Path);

        // Basic routing based on HTTP method and path
        return request.HttpMethod.ToUpperInvariant() switch
        {
            "GET" => await HandleGetRequest(request),
            "POST" => await HandlePostRequest(request),
            "PUT" => await HandlePutRequest(request),
            "DELETE" => await HandleDeleteRequest(request),
            _ => new APIGatewayProxyResponse
            {
                StatusCode = 405,
                Body = JsonSerializer.Serialize(new { error = "Method not allowed" }),
                Headers = new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/json",
                    ["Access-Control-Allow-Origin"] = "*",
                    ["Access-Control-Allow-Headers"] = "Content-Type,X-Amz-Date,Authorization,X-Api-Key,X-Amz-Security-Token",
                    ["Access-Control-Allow-Methods"] = "GET,POST,PUT,DELETE,OPTIONS"
                }
            }
        };
    }

    private async Task<APIGatewayProxyResponse> HandleGetRequest(APIGatewayProxyRequest request)
    {
        _logger.LogInformation("Handling GET request for path: {Path}", request.Path);

        var responseBody = new
        {
            message = "Hello from PollyUniverse Agent API",
            path = request.Path,
            method = request.HttpMethod,
            timestamp = DateTime.UtcNow.ToString("O"),
            requestId = request.RequestContext?.RequestId
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

    private async Task<APIGatewayProxyResponse> HandlePutRequest(APIGatewayProxyRequest request)
    {
        _logger.LogInformation("Handling PUT request for path: {Path}", request.Path);

        var responseBody = new
        {
            message = "PUT request processed successfully",
            path = request.Path,
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

    private async Task<APIGatewayProxyResponse> HandleDeleteRequest(APIGatewayProxyRequest request)
    {
        _logger.LogInformation("Handling DELETE request for path: {Path}", request.Path);

        var responseBody = new
        {
            message = "DELETE request processed successfully",
            path = request.Path,
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
}
