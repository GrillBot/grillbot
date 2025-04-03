using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Invites;

public class GuildInvite : Invite
{
    public Guild Guild { get; set; } = null!;
}
