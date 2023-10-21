using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Invites;

public class Invite : InviteBase
{
    /// <summary>
    /// User that created the invite.
    /// </summary>
    public User? Creator { get; set; }

    /// <summary>
    /// Use counter of invite.
    /// </summary>
    public int UsedUsersCount { get; set; }
}
