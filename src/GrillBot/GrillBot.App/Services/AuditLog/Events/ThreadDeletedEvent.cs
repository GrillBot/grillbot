using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ThreadDeletedEvent : AuditEventBase
{
    private Cacheable<SocketThreadChannel, ulong> CachedThread { get; }

    public ThreadDeletedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, Cacheable<SocketThreadChannel, ulong> cachedThread) : base(auditLogService, auditLogWriter)
    {
        CachedThread = cachedThread;
    }

    public override Task<bool> CanProcessAsync()
        => Task.FromResult(true);

    public override async Task ProcessAsync()
    {
        var thread = await CachedThread.GetOrDownloadAsync();
        var guild = await AuditLogService.GetGuildFromChannelAsync(thread, CachedThread.Id);
        if (guild == null) return;

        var auditLog = (await guild.GetAuditLogsAsync(actionType: ActionType.ThreadDelete))
            .FirstOrDefault(o => CachedThread.Id == ((ThreadDeleteAuditLogData)o.Data).ThreadId);
        if (auditLog == null) return;

        var data = new AuditThreadInfo(auditLog.Data as ThreadDeleteAuditLogData);
        var item = new AuditLogDataWrapper(AuditLogItemType.ThreadDeleted, data, guild, thread, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
