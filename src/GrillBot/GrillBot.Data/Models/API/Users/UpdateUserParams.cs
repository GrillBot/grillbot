using System;

namespace GrillBot.Data.Models.API.Users
{
    public class UpdateUserParams
    {
        public Guid? ApiToken { get; set; }
        public bool BotAdmin { get; set; }
        public string Note { get; set; }
        public bool WebAdminAllowed { get; set; }
        public TimeSpan? SelfUnverifyMinimalTime { get; set; }
    }
}
