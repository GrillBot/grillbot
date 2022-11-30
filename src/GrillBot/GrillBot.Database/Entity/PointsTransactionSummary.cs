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

    public DateTime? MergeRangeFrom { get; set; }
    public DateTime? MergeRangeTo { get; set; }
    public int MergedItemsCount { get; set; }
    public bool IsMerged { get; set; }

    [NotMapped]
    public string SummaryId => $"{GuildId}|{UserId}{Day:o}";

    [NotMapped]
    public long TotalPoints => MessagePoints + ReactionPoints;
}
