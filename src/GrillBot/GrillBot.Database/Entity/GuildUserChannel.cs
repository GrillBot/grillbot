using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class GuildUserChannel
{
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ChannelId { get; set; } = null!;

    public GuildChannel Channel { get; set; } = null!;

    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    public Guild? Guild { get; set; }

    [StringLength(30)]
    public string UserId { get; set; } = null!;

    public GuildUser? User { get; set; }

    [Required]
    public long Count { get; set; } = 0;

    [Required]
    public DateTime FirstMessageAt { get; set; }

    [Required]
    public DateTime LastMessageAt { get; set; }
}
