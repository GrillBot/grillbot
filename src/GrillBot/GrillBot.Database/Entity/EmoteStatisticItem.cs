using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class EmoteStatisticItem
{
    [StringLength(255)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string EmoteId { get; set; } = null!;

    [StringLength(30)]
    public string UserId { get; set; } = null!;

    public GuildUser? User { get; set; }

    [Required]
    public long UseCount { get; set; } = 0;

    [Required]
    public DateTime FirstOccurence { get; set; }

    [Required]
    public DateTime LastOccurence { get; set; }

    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild? Guild { get; set; }
}
