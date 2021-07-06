using Discord;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.AuditLog
{
    public class AuditUserInfo
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }

        public AuditUserInfo() { }

        public AuditUserInfo(IUser user)
        {
            Id = user.Id;
            Username = user.Username;
            Discriminator = user.Discriminator;
        }

        public override string ToString() => string.IsNullOrEmpty(Discriminator) ? Username : $"{Username}#{Discriminator}";
    }
}
