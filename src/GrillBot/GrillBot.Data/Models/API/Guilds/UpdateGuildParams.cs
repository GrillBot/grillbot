using System;
using System.Collections.Generic;
using GrillBot.Common.Extensions;
using GrillBot.Common.Infrastructure;
using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Guilds;

public class UpdateGuildParams : IApiObject
{
    [DiscordId]
    public string MuteRoleId { get; set; }

    [DiscordId]
    public string AdminChannelId { get; set; }

    [DiscordId]
    public string EmoteSuggestionChannelId { get; set; }

    [DiscordId]
    public string VoteChannelId { get; set; }

    public RangeParams<DateTime> EmoteSuggestionsValidity { get; set; }

    public Dictionary<string, string> SerializeForLog()
    {
        var result = new Dictionary<string, string>
        {
            { nameof(MuteRoleId), MuteRoleId },
            { nameof(AdminChannelId), AdminChannelId },
            { nameof(EmoteSuggestionChannelId), EmoteSuggestionChannelId },
            { nameof(VoteChannelId), VoteChannelId }
        };

        result.AddApiObject(EmoteSuggestionsValidity, nameof(EmoteSuggestionsValidity));
        return result;
    }
}
