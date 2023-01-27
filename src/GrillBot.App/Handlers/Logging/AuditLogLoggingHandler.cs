using GrillBot.App.Managers;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.Logging;

public class AuditLogLoggingHandler : ILoggingHandler
{
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private IConfiguration Configuration { get; }

    public AuditLogLoggingHandler(AuditLogWriteManager auditLogWriteManager, IConfiguration configuration)
    {
        AuditLogWriteManager = auditLogWriteManager;
        Configuration = configuration.GetSection("Discord:Logging");
    }

    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        if (exception == null || !Configuration.GetValue<bool>("Enabled")) return Task.FromResult(false);
        return Task.FromResult(severity is LogSeverity.Critical or LogSeverity.Error or LogSeverity.Warning);
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public async Task WarningAsync(string source, string message, Exception? exception = null)
    {
        var isWarning = exception != null && LoggingHelper.IsWarning(exception);
        var data = CreateWrapper(isWarning, source, message, exception);
        await AuditLogWriteManager.StoreAsync(data);
    }

    public async Task ErrorAsync(string source, string message, Exception? exception)
    {
        if (exception is ApiException apiEx) exception = apiEx.InnerException;

        var isWarning = exception != null && LoggingHelper.IsWarning(exception);
        var data = CreateWrapper(isWarning, source, message, exception);
        await AuditLogWriteManager.StoreAsync(data);
    }

    private static AuditLogDataWrapper CreateWrapper(bool isWarning, string source, string message, Exception? exception)
    {
        var type = isWarning ? AuditLogItemType.Warning : AuditLogItemType.Error;
        var severity = isWarning ? LogSeverity.Warning : LogSeverity.Error;
        var data = new LogMessage(severity, source, message, exception).ToString(padSource: 50, prependTimestamp: false);

        return new AuditLogDataWrapper(type, data);
    }
}
