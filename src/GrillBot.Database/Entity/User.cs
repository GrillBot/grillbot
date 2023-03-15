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
    public int Flags { get; set; } = 0;

    public DateTime? Birthday { get; set; }

    [NotMapped]
    public bool BirthdayAcceptYear => Birthday != null && Birthday.Value.Year != 1;

    public string? Note { get; set; }

    [StringLength(32)]
    [MinLength(2)]
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    [StringLength(4)]
    public string Discriminator { get; set; } = null!;

    public UserStatus Status { get; set; }

    public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    
    [StringLength(50)]
    public string? Language { get; set; }

    public ISet<GuildUser> Guilds { get; set; }
    public ISet<RemindMessage> IncomingReminders { get; set; }
    public ISet<RemindMessage> OutgoingReminders { get; set; }
    public ISet<GuildUserChannel> Channels { get; set; }
    public ISet<SearchItem> SearchItems { get; set; }

    public User()
    {
        Guilds = new HashSet<GuildUser>();
        IncomingReminders = new HashSet<RemindMessage>();
        OutgoingReminders = new HashSet<RemindMessage>();
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
        Username = user.IsUser() ? user.Username : user.Username.Cut(32, true)!;
        Discriminator = user.Discriminator;
        Status = user.GetStatus();

        if (user.IsUser())
            Flags &= ~(int)UserFlags.NotUser;
        else
            Flags |= (int)UserFlags.NotUser;
    }

    public string FullName(bool noDiscriminator = false)
        => $"{Username}{(noDiscriminator ? "" : $"#{Discriminator}")}";
}
