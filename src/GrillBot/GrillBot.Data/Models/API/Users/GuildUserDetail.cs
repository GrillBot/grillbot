using Discord;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Data.Models.API.Users;

public class GuildUserDetail
{
    public Guild Guild { get; set; }
    public long Points { get; set; }
    public long GivenReactions { get; set; }
    public long ObtainedReactions { get; set; }
    public string Nickname { get; set; }
    public Invites.Invite UsedInvite { get; set; }
    public List<Invites.InviteBase> CreatedInvites { get; set; }
    public List<UserGuildChannel> Channels { get; set; }
    public bool IsGuildKnown { get; set; }
    public bool IsUserInGuild { get; set; }
    public List<Emotes.EmoteStatItem> Emotes { get; set; }

    public GuildUserDetail() { }

    public GuildUserDetail(Database.Entity.GuildUser user, IGuild guild)
    {
        Guild = new Guild(user.Guild);
        Points = user.Points;
        GivenReactions = user.GivenReactions;
        ObtainedReactions = user.ObtainedReactions;
        Nickname = user.Nickname;
        UsedInvite = user.UsedInvite == null ? null : new Invites.Invite(user.UsedInvite);
        CreatedInvites = user.CreatedInvites.Select(o => new Invites.InviteBase(o)).OrderByDescending(o => o.CreatedAt).ToList();
        IsGuildKnown = guild != null;
        IsUserInGuild = IsGuildKnown && guild.GetUserAsync(Convert.ToUInt64(user.UserId)).Result != null;

        Channels = user.Channels
            .Select(o => new UserGuildChannel(o))
            .OrderByDescending(o => o.Count)
            .ThenBy(o => o.Channel.Name)
            .ToList();

        Emotes = user.EmoteStatistics
            .Select(o => new Emotes.EmoteStatItem(o))
            .OrderByDescending(o => o.UseCount)
            .ThenByDescending(o => o.LastOccurence)
            .ThenBy(o => o.Name)
            .ToList();
    }
}
