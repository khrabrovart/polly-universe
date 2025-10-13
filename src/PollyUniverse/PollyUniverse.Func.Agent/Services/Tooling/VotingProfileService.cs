using PollyUniverse.Shared.OpenAI.Models;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services;

namespace PollyUniverse.Func.Agent.Services.Tooling;

public class VotingProfileService : IToolingService
{
    private const int IndentSize = 2;

    private readonly IVotingProfileRepository _votingProfileRepository;
    private readonly Func<string, string> _t;

    public VotingProfileService(
        IVotingProfileRepository votingProfileRepository,
        IDictionaryService dictionaryService
        )
    {
        _votingProfileRepository = votingProfileRepository;
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
                        Name = "profile_id",
                        Description = _t("get_voting_profile.params.profile_id"),
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
        var votingProfiles = await _votingProfileRepository.GetAll();

        return string.Join(", ", votingProfiles.Select(vp => vp.Id));
    }

    private async Task<string> GetVotingProfile(Dictionary<string, string> parameters)
    {
        if (!parameters.TryGetValue("profile_id", out var id))
        {
            return _t("get_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(id);

        if (votingProfile == null)
        {
            return _t("get_voting_profile.error.not_found");
        }

        var poll =
            $"""
             FromId: {votingProfile.Poll.FromId},
             PeerId: {(long)votingProfile.Poll.PeerId},
             DayOfWeek: {votingProfile.Poll.DayOfWeek},
             Time: {votingProfile.Poll.Time},
             Timezone: {votingProfile.Poll.Timezone},
             """;

        var sessions = votingProfile.Sessions.Select(s =>
            $"""
             {s.Id}:
               Enabled: {s.Enabled}
               VoteIndex: {s.VoteIndex}
               VoteDelaySeconds: {s.VoteDelaySeconds}
             """);

        return
            $"""
             Id: {votingProfile.Id},
             Enabled: {votingProfile.Enabled},
             Poll:
             {Indent(poll, 1)}
             Sessions:
             {Indent(string.Join(Environment.NewLine, sessions), 1)}
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
