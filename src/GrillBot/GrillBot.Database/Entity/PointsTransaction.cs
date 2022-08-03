using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class PointsTransaction
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
    [StringLength(30)]
    public string MessageId { get; set; } = null!;

    [StringLength(255)]
    public string ReactionId { get; set; } = null!;

    [Required]
    public DateTime AssingnedAt { get; set; } = DateTime.Now;

    [Required]
    public int Points { get; set; }

    public bool IsReaction()
        => !string.IsNullOrEmpty(ReactionId);
}
