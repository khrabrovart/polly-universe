using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Models;
using PollyUniverse.Shared.OpenAI.Models;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services;

namespace PollyUniverse.Func.Agent.Services.Tooling;

public class VotingProfileToolingService : IToolingService
{
    private const int IndentSize = 2;

    private readonly IVotingProfileRepository _votingProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VotingProfileToolingService> _logger;
    private readonly Func<string, string> _t;

    public VotingProfileToolingService(
        IVotingProfileRepository votingProfileRepository,
        IUserRepository userRepository,
        IDictionaryService dictionaryService,
        ILogger<VotingProfileToolingService> logger
        )
    {
        _votingProfileRepository = votingProfileRepository;
        _userRepository = userRepository;
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
                Description = _t("list_voting_profiles.description"),
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
            },
            new OpenAITool
            {
                Name = "enable_voting_profile",
                Description = _t("enable_voting_profile.description"),
                Returns = _t("enable_voting_profile.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _t("enable_voting_profile.params.profile_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = EnableVotingProfile
            },
            new OpenAITool
            {
                Name = "disable_voting_profile",
                Description = _t("disable_voting_profile.description"),
                Returns = _t("disable_voting_profile.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _t("disable_voting_profile.params.profile_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = DisableVotingProfile
            },
            new OpenAITool
            {
                Name = "enable_voting_profile_user",
                Description = _t("enable_voting_profile_user.description"),
                Returns = _t("enable_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _t("enable_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _t("enable_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = EnableVotingProfileUser
            },
            new OpenAITool
            {
                Name = "disable_voting_profile_user",
                Description = _t("disable_voting_profile_user.description"),
                Returns = _t("disable_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _t("disable_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _t("disable_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = DisableVotingProfileUser
            },
            new OpenAITool
            {
                Name = "add_voting_profile_user",
                Description = _t("add_voting_profile_user.description"),
                Returns = _t("add_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _t("add_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _t("add_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "vote_index",
                        Description = _t("add_voting_profile_user.params.vote_index"),
                        Type = "integer",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "vote_delay_seconds",
                        Description = _t("add_voting_profile_user.params.vote_delay_seconds"),
                        Type = "integer",
                        Required = true
                    }
                ],
                Action = AddVotingProfileUser
            },
            new OpenAITool
            {
                Name = "remove_voting_profile_user",
                Description = _t("remove_voting_profile_user.description"),
                Returns = _t("remove_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _t("remove_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _t("remove_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = RemoveVotingProfileUser
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

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("get_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _t("get_voting_profile.error.not_found");
        }

        var userIds = votingProfile.Users.Select(s => s.Id).ToArray();
        var users = await _userRepository.Get(userIds);

        if (users.Length != userIds.Length)
        {
            return _t("get_voting_profile.error.users_not_found");
        }

        var usersData = users.ToDictionary(s => s.Id, s => s);

        var pollStr =
            $"""
             {_t("get_voting_profile.output.poll_from_id")}: {votingProfile.Poll.FromId},
             {_t("get_voting_profile.output.poll_peer_id")}: {(long)votingProfile.Poll.PeerId},
             {_t("get_voting_profile.output.poll_day_of_week")}: {votingProfile.Poll.DayOfWeek},
             {_t("get_voting_profile.output.poll_time")}: {votingProfile.Poll.Time},
             {_t("get_voting_profile.output.poll_timezone")}: {votingProfile.Poll.Timezone},
             """;

        var usersStr = votingProfile.Users.Select(s =>
            $"""
             {_t("get_voting_profile.output.user_id")}: {s.Id}
             {_t("get_voting_profile.output.user_name")}: {usersData[s.Id].Name}
             {_t("get_voting_profile.output.user_gender")}: {usersData[s.Id].Gender}
             {_t("get_voting_profile.output.user_enabled")}: {s.Enabled}
             {_t("get_voting_profile.output.user_vote_index")}: {s.VoteIndex}
             {_t("get_voting_profile.output.user_vote_delay_seconds")}: {s.VoteDelaySeconds}

             """);

        return
            $"""
             {_t("get_voting_profile.output.id")}: {votingProfile.Id},
             {_t("get_voting_profile.output.enabled")}: {votingProfile.Enabled},
             {_t("get_voting_profile.output.description")}: {votingProfile.Description},
             {_t("get_voting_profile.output.poll")}:
             {Indent(pollStr, 1)}
             {_t("get_voting_profile.output.users")}:
             {Indent(string.Join(Environment.NewLine, usersStr), 1)}
             """;
    }

    private async Task<string> EnableVotingProfile(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: enable_voting_profile called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("enable_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _t("enable_voting_profile.error.not_found");
        }

        if (votingProfile.Enabled)
        {
            return _t("enable_voting_profile.error.already_enabled");
        }

        votingProfile.Enabled = true;
        await _votingProfileRepository.Update(votingProfile);

        return _t("enable_voting_profile.output.success");
    }

    private async Task<string> DisableVotingProfile(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: disable_voting_profile called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("disable_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _t("disable_voting_profile.error.not_found");
        }

        if (!votingProfile.Enabled)
        {
            return _t("disable_voting_profile.error.already_disabled");
        }

        votingProfile.Enabled = false;
        await _votingProfileRepository.Update(votingProfile);

        return _t("disable_voting_profile.output.success");
    }

    private async Task<string> EnableVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: enable_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("enable_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _t("enable_voting_profile_user.error.missing_user_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _t("enable_voting_profile_user.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _t("enable_voting_profile_user.error.user_not_found");
        }

        if (user.Enabled)
        {
            return _t("enable_voting_profile_user.error.already_enabled");
        }

        user.Enabled = true;
        await _votingProfileRepository.Update(votingProfile);

        return _t("enable_voting_profile_user.output.success");
    }

    private async Task<string> DisableVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: disable_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("disable_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _t("disable_voting_profile_user.error.missing_user_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _t("disable_voting_profile_user.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _t("disable_voting_profile_user.error.user_not_found");
        }

        if (!user.Enabled)
        {
            return _t("disable_voting_profile_user.error.already_disabled");
        }

        user.Enabled = false;
        await _votingProfileRepository.Update(votingProfile);

        return _t("disable_voting_profile_user.output.success");
    }

    private async Task<string> AddVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: add_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("add_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _t("add_voting_profile_user.error.missing_user_id");
        }

        if (!parameters.TryGetValue("vote_index", out var voteIndexStr) || !int.TryParse(voteIndexStr, out var voteIndex))
        {
            return _t("add_voting_profile_user.error.invalid_vote_index");
        }

        if (!parameters.TryGetValue("vote_delay_seconds", out var voteDelaySecondsStr) || !int.TryParse(voteDelaySecondsStr, out var voteDelaySeconds))
        {
            return _t("add_voting_profile_user.error.invalid_vote_delay_seconds");
        }

        var votingProfileTask = _votingProfileRepository.Get(profileId);
        var userTask = _userRepository.Get(userId);

        await Task.WhenAll(votingProfileTask, userTask);

        var votingProfile = votingProfileTask.Result;
        var user = userTask.Result;

        if (votingProfile == null)
        {
            return _t("add_voting_profile_user.error.profile_not_found");
        }

        if (user == null)
        {
            return _t("add_voting_profile_user.error.user_not_found");
        }

        if (votingProfile.Users.Any(s => s.Id == userId))
        {
            return _t("add_voting_profile_user.error.user_already_exists");
        }

        votingProfile.Users.Add(new VotingProfileUser
        {
            Id = userId,
            Enabled = true,
            VoteIndex = voteIndex,
            VoteDelaySeconds = voteDelaySeconds
        });

        await _votingProfileRepository.Update(votingProfile);

        return _t("add_voting_profile_user.output.success");
    }

    private async Task<string> RemoveVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: remove_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _t("remove_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _t("remove_voting_profile_user.error.missing_user_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _t("remove_voting_profile_user.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _t("remove_voting_profile_user.error.user_not_found");
        }

        votingProfile.Users.Remove(user);
        await _votingProfileRepository.Update(votingProfile);

        return _t("remove_voting_profile_user.output.success");
    }

    private static string Indent(string text, int level)
    {
        var indent = new string(' ', IndentSize * level);
        var lines = text.Split('\n');
        var indentedLines = lines.Select(line => indent + line);

        return string.Join(Environment.NewLine, indentedLines);
    }
}
