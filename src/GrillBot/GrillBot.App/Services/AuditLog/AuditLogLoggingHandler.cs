using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public class AuditLogLoggingHandler : ILoggingHandler
{
    private AuditLogWriter AuditLogWriter { get; }
    private IConfiguration Configuration { get; }

    public AuditLogLoggingHandler(AuditLogWriter auditLogWriter, IConfiguration configuration)
    {
        AuditLogWriter = auditLogWriter;
        Configuration = configuration.GetSection("Discord:Logging");
    }

    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception exception = null)
    {
        if (exception == null || !Configuration.GetValue<bool>("Enabled")) return Task.FromResult(false);
        if (exception is GatewayReconnectException || exception.InnerException is GatewayReconnectException) return Task.FromResult(false);

        return Task.FromResult(severity is LogSeverity.Critical or LogSeverity.Error or LogSeverity.Warning);
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public async Task WarningAsync(string source, string message, Exception exception = null)
    {
        var data = CreateWrapper(true, source, message, exception);
        await AuditLogWriter.StoreAsync(data);
    }

    public async Task ErrorAsync(string source, string message, Exception exception)
    {
        var data = CreateWrapper(false, source, message, exception);
        await AuditLogWriter.StoreAsync(data);
    }

    private static AuditLogDataWrapper CreateWrapper(bool isWarning, string source, string message, Exception exception)
    {
        var type = isWarning ? AuditLogItemType.Warning : AuditLogItemType.Error;
        var severity = isWarning ? LogSeverity.Warning : LogSeverity.Error;
        var data = new LogMessage(severity, source, message, exception).ToString(padSource: 50, prependTimestamp: false);

        return new AuditLogDataWrapper(type, data);
    }
}
