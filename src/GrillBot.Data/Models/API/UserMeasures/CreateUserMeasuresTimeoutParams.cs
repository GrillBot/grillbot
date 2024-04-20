using GrillBot.Core.Infrastructure;
using GrillBot.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.UserMeasures;

public class CreateUserMeasuresTimeoutParams : IDictionaryObject
{
    [Required]
    public DateTime CreatedAtUtc { get; set; }

    [Required]
    [StringLength(32)]
    [DiscordId]
    public string ModeratorId { get; set; } = null!;

    [Required]
    [StringLength(32)]
    [DiscordId]
    public string UserId { get; set; } = null!;

    [Required]
    [StringLength(32)]
    [DiscordId]
    public string GuildId { get; set; } = null!;

    [Required]
    public DateTime ValidTo { get; set; }

    public string Reason { get; set; } = null!;

    public Dictionary<string, string?> ToDictionary()
    {
        return new Dictionary<string, string?>
        {
            { nameof(CreatedAtUtc), CreatedAtUtc.ToString("o") },
            { nameof(ModeratorId), ModeratorId },
            { nameof(UserId), UserId },
            { nameof(GuildId), GuildId },
            { nameof(ValidTo), ValidTo.ToString("o") },
            { nameof(Reason), Reason }
        };
    }
}
