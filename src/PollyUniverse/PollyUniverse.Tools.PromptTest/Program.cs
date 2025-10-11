using dotenv.net;
using PollyUniverse.Shared.OpenAI.Services;

namespace PollyUniverse.Tools.PromptTest;

public class Program
{
    private const string LocalPromptsFolder = "./tmp/prompts";

    public static async Task Main(string[] args)
    {
        DotEnv.Load(options: new DotEnvOptions(envFilePaths: [".env", ".env.dev"]));

        var prompts = new[]
        {
            "shared/system_base",
            "voting/prompt_success",
            "shared/system_format"
        };

        var promptFilePaths = prompts.Select(id => Path.Combine(LocalPromptsFolder, $"{id}.md"));
        var prompt = string.Join(
            Environment.NewLine,
            await Task.WhenAll(promptFilePaths.Select(p => File.ReadAllTextAsync(p))));

        var openAiService = new OpenAIService();
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");

        for (var i = 0; i < 10; i++)
        {
            var response = await TryPrompt(openAiService, apiKey, model, prompt);
            Console.WriteLine(response);
        }
    }

    private static async Task<string> TryPrompt(
        OpenAIService openAiService,
        string apiKey,
        string model,
        string prompt)
    {
        return await openAiService.CompleteChat(apiKey, model, prompt);
    }
}
