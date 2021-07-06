using Discord;
using Discord.WebSocket;

namespace GrillBot.Data.Models.AuditLog
{
    public class UserLeftGuildData
    {
        public int MemberCount { get; set; }
        public bool IsBan { get; set; }
        public string BanReason { get; set; }
        public AuditUserInfo User { get; set; }

        public UserLeftGuildData() { }

        public UserLeftGuildData(int memberCount, bool isBan, string banReason, AuditUserInfo user)
        {
            MemberCount = memberCount;
            IsBan = isBan;
            BanReason = banReason;
            User = user;
        }

        public UserLeftGuildData(int memberCount, bool isBan, string banReason, IUser user)
            : this(memberCount, isBan, banReason, new AuditUserInfo(user)) { }

        public UserLeftGuildData(SocketGuild guild, IUser user, bool isBan, string banReason)
            : this(guild.MemberCount, isBan, banReason, user) { }
    }
}
