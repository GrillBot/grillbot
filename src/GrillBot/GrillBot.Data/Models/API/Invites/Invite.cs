using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Invites
{
    public class Invite : InviteBase
    {
        /// <summary>
        /// User that created the invite.
        /// </summary>
        public User Creator { get; set; }

        /// <summary>
        /// Use counter of invite.
        /// </summary>
        public int UsedUsersCount { get; set; }

        public Invite() { }

        public Invite(Database.Entity.Invite invite) : base(invite)
        {
            Creator = invite.Creator == null ? null : new User(invite.Creator.User);
            UsedUsersCount = invite.UsedUsers?.Count ?? 0;
        }
    }
}
