using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class MemberUpdatedEvent : AuditEventBase
{
    private Cacheable<SocketGuildUser, ulong> Before { get; }
    private SocketGuildUser After { get; }

    private SocketGuild Guild => After.Guild;

    public MemberUpdatedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, Cacheable<SocketGuildUser, ulong> before,
        SocketGuildUser after) : base(auditLogService, auditLogWriter)
    {
        Before = before;
        After = after;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            Before.HasValue &&
            (
                Before.Value.IsDeafened != After.IsDeafened ||
                Before.Value.IsMuted != After.IsMuted ||
                Before.Value.Nickname != After.Nickname
            )
        );
    }

    public override async Task ProcessAsync()
    {
        var auditLog = (await Guild.GetAuditLogsAsync(50, actionType: ActionType.MemberUpdated).FlattenAsync())
            .FirstOrDefault(o => ((MemberUpdateAuditLogData)o.Data).Target.Id == After.Id);
        if (auditLog == null) return;

        var data = new MemberUpdatedData(Before.Value, After);
        var item = new AuditLogDataWrapper(AuditLogItemType.MemberUpdated, data, Guild, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
