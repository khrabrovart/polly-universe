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
    private readonly Func<string, string> _s;

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
        _s = dictionaryService.GetString;
    }

    public IEnumerable<OpenAITool> GetTools()
    {
        return
        [
            new OpenAITool
            {
                Name = "list_voting_profiles",
                Description = _s("list_voting_profiles.description"),
                Returns = _s("list_voting_profiles.returns"),
                Action = _ => GetVotingProfiles()
            },
            new OpenAITool
            {
                Name = "get_voting_profile",
                Description = _s("get_voting_profile.description"),
                Returns = _s("get_voting_profile.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("get_voting_profile.params.profile_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = GetVotingProfile
            },
            new OpenAITool
            {
                Name = "enable_voting_profile",
                Description = _s("enable_voting_profile.description"),
                Returns = _s("enable_voting_profile.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("enable_voting_profile.params.profile_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = EnableVotingProfile
            },
            new OpenAITool
            {
                Name = "disable_voting_profile",
                Description = _s("disable_voting_profile.description"),
                Returns = _s("disable_voting_profile.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("disable_voting_profile.params.profile_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = DisableVotingProfile
            },
            new OpenAITool
            {
                Name = "enable_voting_profile_user",
                Description = _s("enable_voting_profile_user.description"),
                Returns = _s("enable_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("enable_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _s("enable_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = EnableVotingProfileUser
            },
            new OpenAITool
            {
                Name = "disable_voting_profile_user",
                Description = _s("disable_voting_profile_user.description"),
                Returns = _s("disable_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("disable_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _s("disable_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = DisableVotingProfileUser
            },
            new OpenAITool
            {
                Name = "add_voting_profile_user",
                Description = _s("add_voting_profile_user.description"),
                Returns = _s("add_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("add_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _s("add_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "vote_index",
                        Description = _s("add_voting_profile_user.params.vote_index"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "vote_delay_seconds",
                        Description = _s("add_voting_profile_user.params.vote_delay_seconds"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = AddVotingProfileUser
            },
            new OpenAITool
            {
                Name = "remove_voting_profile_user",
                Description = _s("remove_voting_profile_user.description"),
                Returns = _s("remove_voting_profile_user.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("remove_voting_profile_user.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _s("remove_voting_profile_user.params.user_id"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = RemoveVotingProfileUser
            },
            new OpenAITool
            {
                Name = "update_voting_profile_user_vote_index",
                Description = _s("update_voting_profile_user_vote_index.description"),
                Returns = _s("update_voting_profile_user_vote_index.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("update_voting_profile_user_vote_index.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _s("update_voting_profile_user_vote_index.params.user_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "vote_index",
                        Description = _s("update_voting_profile_user_vote_index.params.vote_index"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = UpdateVotingProfileUserVoteIndex
            },
            new OpenAITool
            {
                Name = "update_voting_profile_user_vote_delay",
                Description = _s("update_voting_profile_user_vote_delay.description"),
                Returns = _s("update_voting_profile_user_vote_delay.returns"),
                Parameters =
                [
                    new OpenAIToolParameter
                    {
                        Name = "profile_id",
                        Description = _s("update_voting_profile_user_vote_delay.params.profile_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "user_id",
                        Description = _s("update_voting_profile_user_vote_delay.params.user_id"),
                        Type = "string",
                        Required = true
                    },
                    new OpenAIToolParameter
                    {
                        Name = "vote_delay_seconds",
                        Description = _s("update_voting_profile_user_vote_delay.params.vote_delay_seconds"),
                        Type = "string",
                        Required = true
                    }
                ],
                Action = UpdateVotingProfileUserVoteDelay
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
            return _s("get_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("get_voting_profile.error.not_found");
        }

        var userIds = votingProfile.Users.Select(s => s.Id).ToArray();
        var users = await _userRepository.Get(userIds);

        if (users.Length != userIds.Length)
        {
            return _s("get_voting_profile.error.users_not_found");
        }

        var usersData = users.ToDictionary(s => s.Id, s => s);

        var yesNo = new Dictionary<bool, string>
        {
            [true] = _s("common.yes"),
            [false] = _s("common.no")
        };

        var pollStr =
            $"""
             {_s("get_voting_profile.output.poll_from_id")}: {votingProfile.Poll.FromId},
             {_s("get_voting_profile.output.poll_peer_id")}: {(long)votingProfile.Poll.PeerId},
             {_s("get_voting_profile.output.poll_day_of_week")}: {votingProfile.Poll.DayOfWeek},
             {_s("get_voting_profile.output.poll_time")}: {votingProfile.Poll.Time},
             {_s("get_voting_profile.output.poll_timezone")}: {votingProfile.Poll.Timezone},
             """;

        var usersStr = votingProfile.Users.Select((s, i) =>
            $"""
             {i + 1}.
             {_s("get_voting_profile.output.user_id")}: {s.Id}
             {_s("get_voting_profile.output.user_name")}: {usersData[s.Id].Name}
             {_s("get_voting_profile.output.user_gender")}: {usersData[s.Id].Gender}
             {_s("get_voting_profile.output.user_enabled")}: {yesNo[s.Enabled]}
             {_s("get_voting_profile.output.user_vote_index")}: {s.VoteIndex}
             {_s("get_voting_profile.output.user_vote_delay_seconds")}: {s.VoteDelaySeconds}

             """);

        var output =
            $"""
             {votingProfile.Description}
             {_s("get_voting_profile.output.id")}: {votingProfile.Id},
             {_s("get_voting_profile.output.enabled")}: {yesNo[votingProfile.Enabled]},
             {_s("get_voting_profile.output.poll")}:
             {Indent(pollStr, 1)}
             {_s("get_voting_profile.output.users")}:
             {Indent(string.Join(Environment.NewLine, usersStr), 1)}
             """;

        return output;
    }

    private async Task<string> EnableVotingProfile(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: enable_voting_profile called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("enable_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("enable_voting_profile.error.not_found");
        }

        if (votingProfile.Enabled)
        {
            return _s("enable_voting_profile.error.already_enabled");
        }

        votingProfile.Enabled = true;
        await _votingProfileRepository.Update(votingProfile);

        return _s("enable_voting_profile.output.success");
    }

    private async Task<string> DisableVotingProfile(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: disable_voting_profile called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("disable_voting_profile.error.missing_profile_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("disable_voting_profile.error.not_found");
        }

        if (!votingProfile.Enabled)
        {
            return _s("disable_voting_profile.error.already_disabled");
        }

        votingProfile.Enabled = false;
        await _votingProfileRepository.Update(votingProfile);

        return _s("disable_voting_profile.output.success");
    }

    private async Task<string> EnableVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: enable_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("enable_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _s("enable_voting_profile_user.error.missing_user_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("enable_voting_profile_user.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _s("enable_voting_profile_user.error.user_not_found");
        }

        if (user.Enabled)
        {
            return _s("enable_voting_profile_user.error.already_enabled");
        }

        user.Enabled = true;
        await _votingProfileRepository.Update(votingProfile);

        return _s("enable_voting_profile_user.output.success");
    }

    private async Task<string> DisableVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: disable_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("disable_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _s("disable_voting_profile_user.error.missing_user_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("disable_voting_profile_user.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _s("disable_voting_profile_user.error.user_not_found");
        }

        if (!user.Enabled)
        {
            return _s("disable_voting_profile_user.error.already_disabled");
        }

        user.Enabled = false;
        await _votingProfileRepository.Update(votingProfile);

        return _s("disable_voting_profile_user.output.success");
    }

    private async Task<string> AddVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: add_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("add_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _s("add_voting_profile_user.error.missing_user_id");
        }

        if (!parameters.TryGetValue("vote_index", out var voteIndexStr) || !int.TryParse(voteIndexStr, out var voteIndex))
        {
            return _s("add_voting_profile_user.error.invalid_vote_index");
        }

        if (!parameters.TryGetValue("vote_delay_seconds", out var voteDelaySecondsStr) || !int.TryParse(voteDelaySecondsStr, out var voteDelaySeconds))
        {
            return _s("add_voting_profile_user.error.invalid_vote_delay_seconds");
        }

        var votingProfileTask = _votingProfileRepository.Get(profileId);
        var userTask = _userRepository.Get(userId);

        await Task.WhenAll(votingProfileTask, userTask);

        var votingProfile = votingProfileTask.Result;
        var user = userTask.Result;

        if (votingProfile == null)
        {
            return _s("add_voting_profile_user.error.profile_not_found");
        }

        if (user == null)
        {
            return _s("add_voting_profile_user.error.user_not_found");
        }

        if (votingProfile.Users.Any(s => s.Id == userId))
        {
            return _s("add_voting_profile_user.error.user_already_exists");
        }

        votingProfile.Users.Add(new VotingProfileUser
        {
            Id = userId,
            Enabled = true,
            VoteIndex = voteIndex,
            VoteDelaySeconds = voteDelaySeconds
        });

        await _votingProfileRepository.Update(votingProfile);

        return _s("add_voting_profile_user.output.success");
    }

    private async Task<string> RemoveVotingProfileUser(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: remove_voting_profile_user called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("remove_voting_profile_user.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _s("remove_voting_profile_user.error.missing_user_id");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("remove_voting_profile_user.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _s("remove_voting_profile_user.error.user_not_found");
        }

        votingProfile.Users.Remove(user);
        await _votingProfileRepository.Update(votingProfile);

        return _s("remove_voting_profile_user.output.success");
    }

    private async Task<string> UpdateVotingProfileUserVoteIndex(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: update_voting_profile_user_vote_index called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("update_voting_profile_user_vote_index.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _s("update_voting_profile_user_vote_index.error.missing_user_id");
        }

        if (!parameters.TryGetValue("vote_index", out var voteIndexStr) || !int.TryParse(voteIndexStr, out var voteIndex))
        {
            return _s("update_voting_profile_user_vote_index.error.invalid_vote_index");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("update_voting_profile_user_vote_index.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _s("update_voting_profile_user_vote_index.error.user_not_found");
        }

        if (user.VoteIndex == voteIndex)
        {
            return _s("update_voting_profile_user_vote_index.error.already_set");
        }

        user.VoteIndex = voteIndex;
        await _votingProfileRepository.Update(votingProfile);

        return _s("update_voting_profile_user_vote_index.output.success");
    }

    private async Task<string> UpdateVotingProfileUserVoteDelay(Dictionary<string, string> parameters)
    {
        _logger.LogInformation("AI Tools: update_voting_profile_user_vote_delay called with parameters: {Parameters}", parameters);

        if (!parameters.TryGetValue("profile_id", out var profileId))
        {
            return _s("update_voting_profile_user_vote_delay.error.missing_profile_id");
        }

        if (!parameters.TryGetValue("user_id", out var userId))
        {
            return _s("update_voting_profile_user_vote_delay.error.missing_user_id");
        }

        if (!parameters.TryGetValue("vote_delay_seconds", out var voteDelaySecondsStr) || !int.TryParse(voteDelaySecondsStr, out var voteDelaySeconds))
        {
            return _s("update_voting_profile_user_vote_delay.error.invalid_vote_delay_seconds");
        }

        var votingProfile = await _votingProfileRepository.Get(profileId);

        if (votingProfile == null)
        {
            return _s("update_voting_profile_user_vote_delay.error.profile_not_found");
        }

        var user = votingProfile.Users.FirstOrDefault(s => s.Id == userId);

        if (user == null)
        {
            return _s("update_voting_profile_user_vote_delay.error.user_not_found");
        }

        if (user.VoteDelaySeconds == voteDelaySeconds)
        {
            return _s("update_voting_profile_user_vote_delay.error.already_set");
        }

        user.VoteDelaySeconds = voteDelaySeconds;
        await _votingProfileRepository.Update(votingProfile);

        return _s("update_voting_profile_user_vote_delay.output.success");
    }

    private static string Indent(string text, int level)
    {
        var indent = new string(' ', IndentSize * level);
        var lines = text.Split('\n');
        var indentedLines = lines.Select(line => indent + line);

        return string.Join(Environment.NewLine, indentedLines);
    }
}
