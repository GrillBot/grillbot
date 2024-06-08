using Discord;
using GrillBot.Database.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Extensions;

namespace GrillBot.Database.Entity;

[DebuggerDisplay("{Username} ({Id})")]
public class User
{
    [Key]
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;

    [Required]
    public int Flags { get; set; }

    public DateTime? Birthday { get; set; }

    [NotMapped]
    public bool BirthdayAcceptYear => Birthday != null && Birthday.Value.Year != 1;

    [StringLength(32)]
    [MinLength(2)]
    [Required]
    public string Username { get; set; } = null!;

    public UserStatus Status { get; set; }

    public TimeSpan? SelfUnverifyMinimalTime { get; set; }

    [StringLength(50)]
    public string? Language { get; set; }

    [StringLength(1024)]
    public string? AvatarUrl { get; set; }

    [StringLength(32)]
    public string? GlobalAlias { get; set; }

    public ISet<GuildUser> Guilds { get; set; }
    public ISet<GuildUserChannel> Channels { get; set; }
    public ISet<SearchItem> SearchItems { get; set; }

    public User()
    {
        Guilds = new HashSet<GuildUser>();
        Channels = new HashSet<GuildUserChannel>();
        SearchItems = new HashSet<SearchItem>();
    }

    public bool HaveFlags(UserFlags flags)
        => (Flags & (int)flags) != 0;

    public static User FromDiscord(IUser user)
    {
        var entity = new User { Id = user.Id.ToString() };
        entity.Update(user);

        return entity;
    }

    public User Clone() => (User)MemberwiseClone();

    public void Update(IUser user)
    {
        Username = user.Username.Cut(32, true)!;
        Status = user.GetStatus();
        AvatarUrl = user.GetUserAvatarUrl();
        GlobalAlias = user.GlobalName.Cut(32, true)!;

        if (user.IsUser())
            Flags &= ~(int)UserFlags.NotUser;
        else
            Flags |= (int)UserFlags.NotUser;
    }

    public string GetDisplayName()
        => !string.IsNullOrEmpty(GlobalAlias) ? $"{GlobalAlias} ({Username})" : Username;
}
