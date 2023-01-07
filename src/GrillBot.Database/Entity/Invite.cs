using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace GrillBot.Database.Entity;

[DebuggerDisplay("{Code}")]
public class Invite
{
    [Key]
    [StringLength(10)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Code { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    [StringLength(30)]
    public string? CreatorId { get; set; }

    [StringLength(30)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild? Guild { get; set; }

    public GuildUser? Creator { get; set; }

    public ISet<GuildUser> UsedUsers { get; set; }

    public Invite()
    {
        UsedUsers = new HashSet<GuildUser>();
    }
}
