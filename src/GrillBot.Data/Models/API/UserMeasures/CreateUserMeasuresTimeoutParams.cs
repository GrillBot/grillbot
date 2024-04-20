using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.UserMeasures;

public class CreateUserMeasuresTimeoutParams : IDictionaryObject
{
    public long TimeoutId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    [StringLength(32)]
    [DiscordId]
    public string ModeratorId { get; set; } = null!;

    [StringLength(32)]
    [DiscordId]
    public string TargetUserId { get; set; } = null!;

    [StringLength(32)]
    [DiscordId]
    public string GuildId { get; set; } = null!;

    public DateTime ValidToUtc { get; set; }
    public string Reason { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(TimeoutId), TimeoutId.ToString() },
            { nameof(CreatedAtUtc), CreatedAtUtc.ToString("o") },
            { nameof(ModeratorId), ModeratorId },
            { nameof(TargetUserId), TargetUserId },
            { nameof(GuildId), GuildId },
            { nameof(ValidToUtc), ValidToUtc.ToString("o") },
            { nameof(Reason), Reason }
        };
    }
}
