using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.GrillBot.Models.Events.Errors;
using Microsoft.Extensions.Logging;
using Serilog.Data;

namespace GrillBot.App.Handlers.RabbitMQ;

public class RabbitHandlerErrorHandler(
    ILoggerFactory loggerFactory,
    IRabbitPublisher _rabbitPublisher
) : RabbitMessageHandlerBase<RabbitErrorMessage>(loggerFactory)
{
    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(
        RabbitErrorMessage message,
        ICurrentUserProvider currentUser,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default
    )
    {
        await PublishErrorNotificationAsync(message);
        await PublishAuditLogMessageAsync(message);
        return RabbitConsumptionResult.Success;
    }

    private async Task PublishErrorNotificationAsync(RabbitErrorMessage payload)
    {
        var notificationFields = CreateErrorNotificationFields(payload);
        var notification = new ErrorNotificationPayload("Při zpracování zprávy z RabbitMQ došlo k chybě.", notificationFields, null);

        await _rabbitPublisher.PublishAsync(notification);
    }

    private static IEnumerable<ErrorNotificationField> CreateErrorNotificationFields(RabbitErrorMessage payload)
    {
        yield return new ErrorNotificationField("Služba", payload.AssemblyName, true);
        yield return new ErrorNotificationField("Topic", payload.TopicName, true);
        yield return new ErrorNotificationField("Fronta", payload.QueueName, true);
        yield return new ErrorNotificationField("Handler", payload.HandlerType, false);

        if (!string.IsNullOrEmpty(payload.RawMessage))
            yield return new ErrorNotificationField("Délka zprávy", payload.RawMessage.Length.Bytes().ToString(), true);

        if (payload.Headers.Count > 0)
            yield return new ErrorNotificationField("Počet hlaviček", payload.Headers.Count.ToString(), true);

        yield return new ErrorNotificationField("Zkrácený obsah chyby", payload.Exception.Cut(500)!, false);
    }

    private async Task PublishAuditLogMessageAsync(RabbitErrorMessage payload)
    {
        var logRequest = new LogRequest(LogType.Error, DateTime.UtcNow)
        {
            LogMessage = new LogMessageRequest
            {
                Message = BuildErrorMessage(payload),
                Source = $"RabbitHandler/{payload.TopicName}/{payload.QueueName}/{payload.HandlerType}",
                SourceAppName = payload.AssemblyName
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
    }

    private static string BuildErrorMessage(RabbitErrorMessage payload)
    {
        var builder = new StringBuilder(payload.Exception);

        if (!string.IsNullOrEmpty(payload.RawMessage))
            builder.AppendLine().AppendLine("Message:").AppendLine(payload.RawMessage);

        if (payload.Headers.Count > 0)
        {
            builder.AppendLine().AppendLine("Headers:");
            foreach (var (key, value) in payload.Headers)
                builder.AppendFormat("- {0}: {1}", key, value).AppendLine();
        }

        return builder.ToString();
    }
}
