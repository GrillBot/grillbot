using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using System.Collections.Generic;
using GrillBot.Data.Models.API.Unverify;

namespace GrillBot.Data.Models.API.Users;

public class GuildUserDetail
{
    public Guild Guild { get; set; } = null!;
    public long GivenReactions { get; set; }
    public long ObtainedReactions { get; set; }
    public string? Nickname { get; set; }
    public Invites.Invite? UsedInvite { get; set; }
    public List<Invites.InviteBase> CreatedInvites { get; set; } = new();
    public List<UserGuildChannel> Channels { get; set; } = new();
    public bool IsGuildKnown { get; set; }
    public bool IsUserInGuild { get; set; }
    public List<Emotes.EmoteStatItem> Emotes { get; set; } = new();
    public UnverifyInfo? Unverify { get; set; }
    public List<string> NicknameHistory { get; set; } = new();
    public List<Channel> VisibleChannels { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
    public bool HavePointsTransaction { get; set; }
    public List<UserMeasuresItem> UserMeasures { get; set; } = new();
}
