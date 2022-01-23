using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public abstract class AuditEventBase
{
    private AuditLogService AuditLogService { get; }

    protected AuditEventBase(AuditLogService auditLogService)
    {
        AuditLogService = auditLogService;
    }

    public abstract Task<bool> CanProcessAsync();
    public abstract Task ProcessAsync();

    protected Task StoreItemAsync(AuditLogItemType type, IGuild guild, IChannel channel, IUser processedUser, string data, object auditLogItemId = null,
        CancellationToken? cancellationToken = null, List<AuditLogFileMeta> attachments = null)
    {
        return AuditLogService.StoreItemAsync(type, guild, channel, processedUser, data, auditLogItemId, cancellationToken, attachments);
    }
}
