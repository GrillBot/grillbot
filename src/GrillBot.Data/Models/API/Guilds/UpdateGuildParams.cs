using System;
using System.Collections.Generic;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using GrillBot.Database.Models;

namespace GrillBot.Data.Models.API.Guilds;

public class UpdateGuildParams : IDictionaryObject
{
    [DiscordId]
    public string? MuteRoleId { get; set; }

    [DiscordId]
    public string? AdminChannelId { get; set; }

    [DiscordId]
    public string? EmoteSuggestionChannelId { get; set; }

    [DiscordId]
    public string? VoteChannelId { get; set; }

    [DiscordId]
    public string? BotRoomChannelId { get; set; }

    public RangeParams<DateTime>? EmoteSuggestionsValidity { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(MuteRoleId), MuteRoleId },
            { nameof(AdminChannelId), AdminChannelId },
            { nameof(EmoteSuggestionChannelId), EmoteSuggestionChannelId },
            { nameof(VoteChannelId), VoteChannelId },
            { nameof(BotRoomChannelId), BotRoomChannelId }
        };

        result.MergeDictionaryObjects(EmoteSuggestionsValidity, nameof(EmoteSuggestionsValidity));
        return result;
    }
}
