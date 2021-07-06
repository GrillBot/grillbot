using Discord.Rest;

namespace GrillBot.Data.Models.AuditLog
{
    public class ChannelUpdateAuditData
    {
        public AuditChannelInfo Before { get; set; }
        public AuditChannelInfo After { get; set; }

        public ChannelUpdateAuditData() { }

        public ChannelUpdateAuditData(AuditChannelInfo before, AuditChannelInfo after)
        {
            Before = before;
            After = after;
        }

        public ChannelUpdateAuditData(ChannelInfo before, ChannelInfo after)
            : this(new AuditChannelInfo(before), new AuditChannelInfo(after)) { }

        public ChannelUpdateAuditData(ChannelUpdateAuditLogData data)
            : this(data.Before, data.After) { }
    }
}
