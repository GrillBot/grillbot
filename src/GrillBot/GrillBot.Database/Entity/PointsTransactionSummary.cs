using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class PointsTransactionSummary
{
    [Required]
    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild Guild { get; set; } = null!;

    [Required]
    [StringLength(30)]
    public string UserId { get; set; } = null!;

    public GuildUser GuildUser { get; set; } = null!;

    [Required]
    public DateTime Day { get; set; }

    [Required]
    public long MessagePoints { get; set; }

    [Required]
    public long ReactionPoints { get; set; }

    public override int GetHashCode()
    {
        return $"{GuildId}|{UserId}|{Day:o}".GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PointsTransactionSummary summary)
            return false;

        return summary.GuildId == GuildId && summary.UserId == UserId && summary.Day == Day;
    }
}
