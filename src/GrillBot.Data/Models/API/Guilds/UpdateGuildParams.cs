using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;

namespace GrillBot.Data.Models.API.Guilds;

public class UpdateGuildParams : IDictionaryObject
{
    [DiscordId]
    [StringLength(30)]
    public string? MuteRoleId { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? AdminChannelId { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? VoteChannelId { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? BotRoomChannelId { get; set; }

    [DiscordId]
    [StringLength(30)]
    public string? AssociationRoleId { get; set; }

    public Dictionary<string, string?> ToDictionary()
    {
        var result = new Dictionary<string, string?>
        {
            { nameof(MuteRoleId), MuteRoleId },
            { nameof(AdminChannelId), AdminChannelId },
            { nameof(VoteChannelId), VoteChannelId },
            { nameof(BotRoomChannelId), BotRoomChannelId },
            { nameof(AssociationRoleId), AssociationRoleId }
        };

        return result;
    }
}
