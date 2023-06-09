using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers.Logging;

public class AuditLogLoggingHandler : ILoggingHandler
{
    private IConfiguration Configuration { get; }
    private IServiceProvider ServiceProvider { get; }

    public AuditLogLoggingHandler(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        Configuration = configuration.GetSection("Discord:Logging");
        ServiceProvider = serviceProvider;
    }

    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        if (exception is null || !Configuration.GetValue<bool>("Enabled")) return Task.FromResult(false);
        return Task.FromResult(severity is LogSeverity.Critical or LogSeverity.Error or LogSeverity.Warning);
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public async Task WarningAsync(string source, string message, Exception? exception = null)
        => await ErrorAsync(source, message, exception);

    public async Task ErrorAsync(string source, string message, Exception? exception)
    {
        IUser? user = null;
        switch (exception)
        {
            case ApiException apiEx:
                exception = apiEx.InnerException;
                user = apiEx.LoggedUser;
                break;
            case JobException jobException:
                exception = jobException.InnerException;
                user = jobException.LoggedUser;
                break;
        }

        var isWarning = exception != null && LoggingHelper.IsWarning(exception);
        var logRequest = CreateLogRequest(isWarning, source, message, exception, user);
        await SendLogRequestAsync(logRequest);
    }

    private static LogRequest CreateLogRequest(bool isWarning, string source, string message, Exception? exception, IUser? user)
    {
        var severity = isWarning ? LogSeverity.Warning : LogSeverity.Error;
        var logMessage = new LogMessage(severity, source, message, exception)
            .ToString(padSource: 50, prependTimestamp: false);

        return new LogRequest
        {
            Type = isWarning ? LogType.Warning : LogType.Error,
            LogMessage = new LogMessageRequest
            {
                Message = logMessage,
                Severity = severity
            },
            CreatedAt = DateTime.UtcNow,
            UserId = user?.Id.ToString()
        };
    }

    private async Task SendLogRequestAsync(LogRequest request)
    {
        using var scope = ServiceProvider.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<IAuditLogServiceClient>();
        await client.CreateItemsAsync(new List<LogRequest> { request });
    }
}
