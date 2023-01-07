using GrillBot.Database.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrillBot.Database.Entity;

public class UnverifyLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required]
    public UnverifyOperation Operation { get; set; }

    [Required]
    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild? Guild { get; set; }

    [Required]
    [StringLength(30)]
    public string FromUserId { get; set; } = null!;

    public GuildUser? FromUser { get; set; }

    [Required]
    [StringLength(30)]
    public string ToUserId { get; set; } = null!;

    public GuildUser? ToUser { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public string Data { get; set; } = null!;

    public Unverify? Unverify { get; set; }
}