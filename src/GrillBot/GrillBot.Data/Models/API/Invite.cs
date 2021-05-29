using System;

namespace GrillBot.Data.Models.API
{
    public class Invite
    {
        public string Code { get; set; }
        public DateTime? CreatedAt { get; set; }
        public User Creator { get; set; }
        public int UsedUsersCount { get; set; }
    }
}
