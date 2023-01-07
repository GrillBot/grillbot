namespace GrillBot.Data.Models.API.Users;

/// <summary>
/// Guild variant of user
/// </summary>
public class GuildUser : User
{
    /// <summary>
    /// Used invite
    /// </summary>
    public Invites.Invite UsedInvite { get; set; }

    /// <summary>
    /// Points count
    /// </summary>
    public long Points { get; set; }

    /// <summary>
    /// Given reactions count
    /// </summary>
    public long GivenReactions { get; set; }

    /// <summary>
    /// Obtained reactions count.
    /// </summary>
    public long ObtainedReactions { get; set; }

    /// <summary>
    /// Nickname.
    /// </summary>
    public string Nickname { get; set; }
}
