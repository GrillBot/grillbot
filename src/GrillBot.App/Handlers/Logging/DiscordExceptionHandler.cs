using Discord.Interactions;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Exceptions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Errors;

namespace GrillBot.App.Handlers.Logging;

public class DiscordExceptionHandler(IRabbitPublisher _rabbitPublisher) : ILoggingHandler
{
    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        if (exception is null) return Task.FromResult(false);
        if (severity != LogSeverity.Critical && severity != LogSeverity.Error && severity != LogSeverity.Warning) return Task.FromResult(false);

        var ex = exception is InteractionException ? exception.InnerException! : exception;
        return Task.FromResult(!LoggingHelper.IsWarning(source, ex));
    }

    public Task InfoAsync(string source, string message) => Task.CompletedTask;

    public Task WarningAsync(string source, string message, Exception? exception = null)
        => ErrorAsync(source, message, exception!);

    public Task ErrorAsync(string source, string message, Exception exception)
    {
        var notification = CreateErrorNotification(source, message, exception);
        return _rabbitPublisher.PublishAsync(notification);
    }

    private static ErrorNotificationPayload CreateErrorNotification(string source, string message, Exception exception)
    {
        var payload = new ErrorNotificationPayload();

        switch (exception)
        {
            case ApiException apiException:
                SetApiExceptionInfo(payload, apiException, message);
                break;
            case InteractionException interactionException:
                SetInteractionExceptionInfo(payload, interactionException, message);
                break;
            case JobException jobException:
                SetJobExceptionInfo(payload, jobException, source, message);
                break;
            case FrontendException frontendException:
                SetFrontendExceptionInfo(payload, frontendException, source, message);
                break;
            default:
                SetCommonExceptionInfo(payload, exception, source, message);
                break;
        }

        return payload;
    }

    private static void SetApiExceptionInfo(ErrorNotificationPayload payload, ApiException exception, string? message)
    {
        payload.Title = "Při zpracování požadavku na API došlo k chybě";

        if (!string.IsNullOrEmpty(exception.Path))
            payload.Fields.Add(new("Adresa", exception.Path, false));
        if (!string.IsNullOrEmpty(exception.ControllerInfo))
            payload.Fields.Add(new("Controller", exception.ControllerInfo, false));

        if (exception.LoggedUser is not null)
        {
            payload.UserId = exception.LoggedUser.Id;
            payload.Fields.Add(new("Přihlášený uživatel", exception.LoggedUser.GetFullName(), false));
        }

        var exceptionMessage = CreateExceptionContentMessage(message, exception);
        payload.Fields.Add(new("Obsah chyby", exceptionMessage, false));
    }

    private static void SetInteractionExceptionInfo(ErrorNotificationPayload payload, InteractionException exception, string? message)
    {
        var context = exception.InteractionContext;
        var cmd = exception.CommandInfo;

        payload.Title = "Při provádění příkazu došlo k chybě.";
        payload.UserId = context.User.Id;

        if (context.Guild is not null)
            payload.Fields.Add(new("Server", context.Guild.Name, true));

        payload.Fields.Add(new("Kanál", context.Channel.Name, true));
        payload.Fields.Add(new("Uživatel", context.User.GetFullName(), false));
        payload.Fields.Add(new("Příkaz", $"{cmd.Name} ({cmd.Module}/{cmd.MethodName})", false));

        var exceptionMessage = CreateExceptionContentMessage(message, exception.InnerException!);
        payload.Fields.Add(new("Obsah chyby", exceptionMessage, false));
    }

    private static void SetJobExceptionInfo(ErrorNotificationPayload payload, JobException exception, string source, string? message)
    {
        payload.Title = "Při běhu naplánované úlohy došlo k chybě.";

        payload.Fields.Add(new("Zdroj", source, true));
        payload.Fields.Add(new("Typ", exception.InnerException!.GetType().Name, true));

        if (exception.LoggedUser is not null)
        {
            payload.UserId = exception.LoggedUser.Id;
            payload.Fields.Add(new("Spustil", exception.LoggedUser.GetFullName(), false));
        }

        var exceptionMessage = CreateExceptionContentMessage(message, exception.InnerException!);
        payload.Fields.Add(new("Obsah chyby", exceptionMessage, false));
    }

    private static void SetCommonExceptionInfo(ErrorNotificationPayload payload, Exception exception, string source, string? message)
    {
        payload.Title = "Došlo k neočekávané chybě.";

        payload.Fields.Add(new("Zdroj", source, true));
        payload.Fields.Add(new("Typ", exception.GetType().Name, true));

        var exceptionMessage = CreateExceptionContentMessage(message, exception);
        payload.Fields.Add(new("Obsah chyby", exceptionMessage, false));
    }

    private static void SetFrontendExceptionInfo(ErrorNotificationPayload payload, FrontendException exception, string source, string? message)
    {
        payload.Title = "Došlo k neočekávané chybě na webu.";
        payload.UserId = exception.LoggedUser.Id;

        payload.Fields.Add(new("Zdroj", source, true));
        payload.Fields.Add(new("Typ", exception.GetType().Name, true));

        var exceptionMessage = CreateExceptionContentMessage(message, exception);
        payload.Fields.Add(new("Obsah chyby", exceptionMessage, false));
    }

    private static string CreateExceptionContentMessage(string? message, Exception exception)
    {
        var msg = (!string.IsNullOrEmpty(message) ? message + "\n" : "") + exception.Message;
        return msg.Trim().Cut(EmbedFieldBuilder.MaxFieldValueLength)!;
    }
}
