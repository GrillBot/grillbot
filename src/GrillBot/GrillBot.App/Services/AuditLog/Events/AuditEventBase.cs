using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public abstract class AuditEventBase
{
    protected AuditLogService AuditLogService { get; }

    protected AuditEventBase(AuditLogService auditLogService)
    {
        AuditLogService = auditLogService;
    }

    public abstract Task<bool> CanProcessAsync();
    public abstract Task ProcessAsync();
}
