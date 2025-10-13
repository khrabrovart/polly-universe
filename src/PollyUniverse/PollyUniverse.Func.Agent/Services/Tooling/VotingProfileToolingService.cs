using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.OpenAI.Models;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services;

namespace PollyUniverse.Func.Agent.Services.Tooling;

public class VotingProfileToolingService : IToolingService
{
    private const int IndentSize = 2;

    private readonly IVotingProfileRepository _votingProfileRepository;
    private readonly ISessionMetadataRepository _sessionMetadataRepository;
    private readonly ILogger<VotingProfileToolingService> _logger;
    private readonly Func<string, string> _t;

    public VotingProfileToolingService(
        IVotingProfileRepository votingProfileRepository,
        ISessionMetadataRepository sessionMetadataRepository,
        IDictionaryService dictionaryService,
        ILogger<VotingProfileToolingService> logger
        )
    {
        _votingProfileRepository = votingProfileRepository;
        _sessionMetadataRepository = sessionMetadataRepository;
        _logger = logger;
        _t = dictionaryService.GetString;
    }

    public IEnumerable<OpenAITool> GetTools()
    {
        return
        [
            new OpenAITool
            {
                Name = "list_voting_profiles",
                Description =  _t("list_voting_profiles.description"),
                Returns = _t("list_voting_profiles.returns"),
                Action = _ => GetVotingProfiles()
            },
            new OpenAITool
            {
                Name = "get_voting_profile",
                Description = _t("get_voting_profile.description"),
                Returns = _t("get_voting_profile.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "id",
                        Description = _t("get_voting_profile.params.id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = GetVotingProfile
            }
        ];
    }

    private async Task<string> GetVotingProfiles()
    {
        _logger.LogInformation("AI Tools: list_voting_profiles called");

        var votingProfiles = await _votingProfileRepository.GetAll();

        return string.Join(", ", votingProfiles.Select(vp => vp.Id));
    }

    private async Task<string> GetVotingProfile(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: get_voting_profile called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("id", out var id))
        {
            return _t("get_voting_profile.error.missing_id");
        }

        var votingProfile = await _votingProfileRepository.Get(id);

        if (votingProfile == null)
        {
            return _t("get_voting_profile.error.not_found");
        }

        var sessionIds = votingProfile.Sessions.Select(s => s.Id).ToArray();
        var sessions = await _sessionMetadataRepository.Get(sessionIds);

        if (sessions.Length != sessionIds.Length)
        {
            return _t("get_voting_profile.error.sessions_not_found");
        }

        var pollStr =
            $"""
             {_t("get_voting_profile.output.poll_from_id")}: {votingProfile.Poll.FromId},
             {_t("get_voting_profile.output.poll_peer_id")}: {(long)votingProfile.Poll.PeerId},
             {_t("get_voting_profile.output.poll_day_of_week")}: {votingProfile.Poll.DayOfWeek},
             {_t("get_voting_profile.output.poll_time")}: {votingProfile.Poll.Time},
             {_t("get_voting_profile.output.poll_timezone")}: {votingProfile.Poll.Timezone},
             """;

        var sessionsStr = sessions.Select(s =>
            $"""
             {s.Id}:
               {_t("get_voting_profile.output.session_user_name")}: {s.User.Name}
               {_t("get_voting_profile.output.session_user_gender")}: {s.User.Gender}
             """);

        return
            $"""
             {_t("get_voting_profile.output.id")}: {votingProfile.Id},
             {_t("get_voting_profile.output.enabled")}: {votingProfile.Enabled},
             {_t("get_voting_profile.output.description")}: {votingProfile.Description},
             {_t("get_voting_profile.output.poll")}:
             {Indent(pollStr, 1)}
             {_t("get_voting_profile.output.sessions")}:
             {Indent(string.Join(Environment.NewLine, sessionsStr), 1)}
             """;
    }

    private static string Indent(string text, int level)
    {
        var indent = new string(' ', IndentSize * level);
        var lines = text.Split('\n');
        var indentedLines = lines.Select(line => indent + line);

        return string.Join(Environment.NewLine, indentedLines);
    }
}
