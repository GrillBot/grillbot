using Discord;
using Discord.Rest;

namespace GrillBot.Data.Models.AuditLog
{
    public class OverwriteUpdatedData
    {
        public AuditOverwriteInfo Before { get; set; }
        public AuditOverwriteInfo After { get; set; }

        public OverwriteUpdatedData() { }

        public OverwriteUpdatedData(AuditOverwriteInfo before, AuditOverwriteInfo after)
        {
            Before = before;
            After = after;
        }

        public OverwriteUpdatedData(Overwrite before, Overwrite after)
            : this(new AuditOverwriteInfo(before), new AuditOverwriteInfo(after)) { }

        public OverwriteUpdatedData(OverwriteUpdateAuditLogData data)
            : this(new Overwrite(data.OverwriteTargetId, data.OverwriteType, data.OldPermissions), new Overwrite(data.OverwriteTargetId, data.OverwriteType, data.NewPermissions)) { }
    }
}
