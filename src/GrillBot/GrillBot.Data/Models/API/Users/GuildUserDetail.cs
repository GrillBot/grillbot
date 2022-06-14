using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using System.Collections.Generic;
using GrillBot.Data.Models.API.Unverify;

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
    public UnverifyInfo Unverify { get; set; }
}
