using Discord.Interactions;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
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
            case InteractionException interactionException:
                exception = interactionException.InnerException;
                user = interactionException.InteractionContext.User;
                break;
            case FrontendException frontendException:
                user = frontendException.LoggedUser;
                break;
        }

        var isWarning = exception != null && LoggingHelper.IsWarning(source, exception);
        var logRequest = CreateLogRequest(isWarning, source, message, exception, user);
        var payload = new CreateItemsPayload(logRequest);

        using var scope = ServiceProvider.CreateScope();
        var rabbitPublisher = scope.ServiceProvider.GetRequiredService<IRabbitMQPublisher>();

        await rabbitPublisher.PublishAsync(payload, new());
    }

    private static LogRequest CreateLogRequest(bool isWarning, string source, string message, Exception? exception, IUser? user)
    {
        var severity = isWarning ? LogSeverity.Warning : LogSeverity.Error;
        var type = isWarning ? LogType.Warning : LogType.Error;
        var logMessage = new LogMessage(severity, source, message, exception)
            .ToString(prependTimestamp: false, padSource: 20);

        return new LogRequest(type, DateTime.UtcNow, null, user?.Id.ToString())
        {
            LogMessage = new LogMessageRequest
            {
                Message = logMessage,
                Severity = severity,
                SourceAppName = "GrillBot" + (exception is FrontendException ? ".Web" : ".App"),
                Source = source
            }
        };
    }
}
