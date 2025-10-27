using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.OpenAI.Models;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services;

namespace PollyUniverse.Func.Agent.Services.Tooling;

public class UserToolingService : IToolingService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserToolingService> _logger;
    private readonly Func<string, string> _s;

    public UserToolingService(
        IUserRepository userRepository,
        IDictionaryService dictionaryService,
        ILogger<UserToolingService> logger
        )
    {
        _userRepository = userRepository;
        _logger = logger;
        _s = dictionaryService.GetString;
    }

    public IEnumerable<OpenAITool> GetTools()
    {
        return
        [
            new OpenAITool
            {
                Name = "list_users",
                Description = _s("list_users.description"),
                Returns = _s("list_users.returns"),
                Action = _ => ListUsers()
            }
        ];
    }

    private async Task<string> ListUsers()
    {
        _logger.LogInformation("AI Tools: list_users called");

        var users = await _userRepository.GetAll();

        return string.Join(", ", users.Select(u => u.Id));
    }
}
