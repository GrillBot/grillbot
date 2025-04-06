using Discord;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Extensions;
using GrillBot.Common.Extensions;

namespace GrillBot.Database.Entity;

[DebuggerDisplay("{Nickname} ({UserId}/{GuildId})")]
public class GuildUser
{
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string GuildId { get; set; } = null!;

    [ForeignKey(nameof(GuildId))]
    public Guild? Guild { get; set; }

    [Required]
    public long GivenReactions { get; set; } = 0;

    [Required]
    public long ObtainedReactions { get; set; } = 0;

    public Unverify? Unverify { get; set; }

    [StringLength(32)]
    [MinLength(2)]
    public string? Nickname { get; set; }

    public bool IsInGuild { get; set; }

    public ISet<GuildUserChannel> Channels { get; set; }
    public ISet<Nickname> Nicknames { get; set; }

    [NotMapped]
    public string? DisplayName
        => string.IsNullOrEmpty(Nickname) ? User?.Username : $"{Nickname} ({User?.Username})";

    public GuildUser()
    {
        Channels = new HashSet<GuildUserChannel>();
        Nicknames = new HashSet<Nickname>();
    }

    public static GuildUser FromDiscord(IGuild guild, IGuildUser user)
    {
        var entity = new GuildUser
        {
            GuildId = guild.Id.ToString(),
            UserId = user.Id.ToString()
        };
        entity.Update(user);

        return entity;
    }

    public void Update(IGuildUser user)
    {
        Nickname = (user.IsUser() ? user.Nickname : user.Nickname?.Cut(32, true))?.RemoveInvalidUnicodeChars();
        IsInGuild = true;
        User?.Update(user);
    }
}
