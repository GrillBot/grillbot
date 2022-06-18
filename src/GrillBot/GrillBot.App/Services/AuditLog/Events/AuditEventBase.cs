namespace GrillBot.App.Services.AuditLog.Events;

public abstract class AuditEventBase
{
    protected AuditLogService AuditLogService { get; }
    protected AuditLogWriter AuditLogWriter { get; }

    protected AuditEventBase(AuditLogService auditLogService, AuditLogWriter auditLogWriter)
    {
        AuditLogService = auditLogService;
        AuditLogWriter = auditLogWriter;
    }

    public abstract Task<bool> CanProcessAsync();
    public abstract Task ProcessAsync();
}
