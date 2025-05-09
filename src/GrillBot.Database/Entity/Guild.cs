﻿using Discord;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;

namespace GrillBot.Database.Entity;

[DebuggerDisplay("{Name} ({Id})")]
public class Guild
{
    [Key]
    [StringLength(30)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [MinLength(2)]
    public string Name { get; set; } = null!;

    [StringLength(30)]
    public string? MuteRoleId { get; set; }

    [StringLength(30)]
    public string? AdminChannelId { get; set; }

    [StringLength(30)]
    public string? BoosterRoleId { get; set; }

    [StringLength(30)]
    public string? VoteChannelId { get; set; }

    [StringLength(30)]
    public string? BotRoomChannelId { get; set; }

    [StringLength(30)]
    public string? AssociationRoleId { get; set; }

    public ISet<GuildUser> Users { get; set; }
    public ISet<GuildChannel> Channels { get; set; }
    public ISet<Unverify> Unverifies { get; set; }
    public ISet<UnverifyLog> UnverifyLogs { get; set; }

    public Guild()
    {
        Users = new HashSet<GuildUser>();
        Channels = new HashSet<GuildChannel>();
        Unverifies = new HashSet<Unverify>();
        UnverifyLogs = new HashSet<UnverifyLog>();
    }

    public static Guild FromDiscord(IGuild guild)
    {
        var entity = new Guild { Id = guild.Id.ToString() };
        entity.Update(guild);

        return entity;
    }

    public void Update(IGuild guild)
    {
        Name = guild.Name;
        BoosterRoleId = guild.Roles.FirstOrDefault(o => o.Tags?.IsPremiumSubscriberRole == true)?.Id.ToString();
    }
}
