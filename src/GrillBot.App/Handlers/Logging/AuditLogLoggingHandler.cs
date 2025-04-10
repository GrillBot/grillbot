﻿using Discord.Interactions;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Handlers.Logging;

public class AuditLogLoggingHandler(
    IConfiguration _configuration,
    IServiceProvider _serviceProvider
) : ILoggingHandler
{
    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        if (exception is null || !_configuration.GetValue<bool>("Discord:Logging:Enabled")) return Task.FromResult(false);
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
        var payload = new CreateItemsMessage(logRequest);

        using var scope = _serviceProvider.CreateScope();
        var rabbitPublisher = scope.ServiceProvider.GetRequiredService<IRabbitPublisher>();

        await rabbitPublisher.PublishAsync(payload);
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
                SourceAppName = "GrillBot" + (exception is FrontendException ? ".Web" : ".App"),
                Source = source
            }
        };
    }
}
