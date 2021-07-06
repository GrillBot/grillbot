using Discord.WebSocket;

namespace GrillBot.Data.Models.AuditLog
{
    public class UserJoinedAuditData
    {
        public int MemberCount { get; set; }

        public UserJoinedAuditData() { }

        public UserJoinedAuditData(int memberCount)
        {
            MemberCount = memberCount;
        }

        public UserJoinedAuditData(SocketGuild guild) : this(guild.MemberCount) { }
    }
}
