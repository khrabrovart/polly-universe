using OpenAI.Chat;
using PollyUniverse.Shared.OpenAI.Models;
using System.Text.Json;

namespace PollyUniverse.Shared.OpenAI.Services;

public interface IOpenAIService
{
    Task<string> CompleteChat(
        string apiKey,
        string model,
        string systemMessage);

    Task<string> CompleteChat(
        string apiKey,
        string model,
        string systemMessage,
        string[] userMessages);

    Task<string> CompleteChat(
        string apiKey,
        string model,
        string systemMessage,
        string[] userMessages,
        OpenAITool[] tools);
}

public class OpenAIChatService : IOpenAIService
{
    public async Task<string> CompleteChat(
        string apiKey,
        string model,
        string systemMessage)
    {
        var client = new ChatClient(model, apiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemMessage)
        };

        var response = await client.CompleteChatAsync(messages);

        return response.Value.Content[0].Text;
    }

    public async Task<string> CompleteChat(
        string apiKey,
        string model,
        string systemMessage,
        string[] userMessages)
    {
        var client = new ChatClient(model, apiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemMessage)
        };

        messages.AddRange(userMessages.Select(m => new UserChatMessage(m)));

        var response = await client.CompleteChatAsync(messages);

        return response.Value.Content[0].Text;
    }

    public async Task<string> CompleteChat(
        string apiKey,
        string model,
        string systemMessage,
        string[] userMessages,
        OpenAITool[] tools)
    {
        var client = new ChatClient(model, apiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemMessage)
        };

        messages.AddRange(userMessages.Select(m => new UserChatMessage(m)));

        var opts = new ChatCompletionOptions();
        var toolsDictionary = new Dictionary<string, OpenAITool>();

        foreach (var tool in tools)
        {
            var parameters = ParametersToBinaryData(tool.Parameters);
            var description = string.Join(" ", tool.Description, tool.Returns);
            var functionTool = ChatTool.CreateFunctionTool(tool.Name, description, parameters, functionSchemaIsStrict: true);

            opts.Tools.Add(functionTool);
            toolsDictionary[tool.Name] = tool;
        }

        while (true)
        {
            var response = await client.CompleteChatAsync(messages, opts);

            if (response.Value.ToolCalls?.Count > 0)
            {
                messages.Add(new AssistantChatMessage(response.Value));

                foreach (var toolCall in response.Value.ToolCalls)
                {
                    var functionName = toolCall.FunctionName;
                    var functionArguments = toolCall.FunctionArguments;

                    var selectedTool = toolsDictionary[functionName];
                    var parameters = BinaryDataToArguments(functionArguments);
                    var toolResult = await selectedTool.Action(parameters);

                    messages.Add(new ToolChatMessage(toolCall.Id, toolResult));
                }
            }
            else
            {
                return response.Value.Content[0].Text;
            }
        }
    }

    private static BinaryData ParametersToBinaryData(OpenAIToolParameter[] parameters)
    {
        object obj;

        if (parameters is null || parameters.Length == 0)
        {
            obj = new
            {
                type = "object",
                properties = new { },
                additionalProperties = false
            };
        }
        else
        {
            var properties = parameters.ToDictionary(
                parameter => parameter.Name,
                parameter => new
                {
                    type = parameter.Type,
                    description = parameter.Description
                });

            var required = parameters
                .Where(p => p.Required)
                .Select(p => p.Name)
                .ToArray();

            obj = new
            {
                type = "object",
                properties,
                required,
                additionalProperties = false
            };
        }

        return BinaryData.FromObjectAsJson(obj);
    }

    private static Dictionary<string, string> BinaryDataToArguments(BinaryData data)
    {
        return data is null
            ? new Dictionary<string, string>()
            : JsonSerializer.Deserialize<Dictionary<string, string>>(data);
    }
}
