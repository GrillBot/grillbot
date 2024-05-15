using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.RabbitMQ.Payloads.ErrorHandling;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.GrillBot.Models.Events.Errors;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class RabbitHandlerErrorHandler : BaseRabbitMQHandler<RabbitHandlerErrorPayload>
{
    public override string QueueName => new RabbitHandlerErrorPayload().QueueName;

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public RabbitHandlerErrorHandler(ILoggerFactory loggerFactory, IRabbitMQPublisher publisher) : base(loggerFactory)
    {
        _rabbitPublisher = publisher;
    }

    protected override async Task HandleInternalAsync(RabbitHandlerErrorPayload payload, Dictionary<string, string> headers)
    {
        await PublishErrorNotificationAsync(payload);
        await PublishAuditLogMessageAsync(payload);
    }

    private async Task PublishErrorNotificationAsync(RabbitHandlerErrorPayload payload)
    {
        var notificationFields = CreateErrorNotificationFields(payload);
        var notification = new ErrorNotificationPayload("Při zpracování zprávy z RabbitMQ došlo k chybě.", notificationFields, null);

        await _rabbitPublisher.PublishAsync(notification);
    }

    private static IEnumerable<ErrorNotificationField> CreateErrorNotificationFields(RabbitHandlerErrorPayload payload)
    {
        yield return new ErrorNotificationField("Služba", payload.AssemblyName, true);
        yield return new ErrorNotificationField("Fronta", payload.ReceiverQueueName, true);
        yield return new ErrorNotificationField("Handler", payload.HandlerType, false);
        yield return new ErrorNotificationField("Typ zprávy", payload.PayloadType, false);

        if (!string.IsNullOrEmpty(payload.Message))
            yield return new ErrorNotificationField("Délka zprávy", payload.Message.Length.Bytes().ToString(), true);

        if (payload.Headers.Count > 0)
            yield return new ErrorNotificationField("Počet hlaviček", payload.Headers.Count.ToString(), true);

        yield return new ErrorNotificationField("Zkrácený obsah chyby", payload.FullException.Cut(500)!, false);
    }

    private async Task PublishAuditLogMessageAsync(RabbitHandlerErrorPayload payload)
    {
        var logRequest = new LogRequest(LogType.Error, DateTime.UtcNow)
        {
            LogMessage = new LogMessageRequest
            {
                Message = BuildErrorMessage(payload),
                Severity = LogSeverity.Error,
                Source = $"RabbitHandler/{payload.ReceiverQueueName}/{payload.HandlerType}({payload.PayloadType})",
                SourceAppName = payload.AssemblyName
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new List<LogRequest> { logRequest }));
    }

    private static string BuildErrorMessage(RabbitHandlerErrorPayload payload)
    {
        var builder = new StringBuilder(payload.FullException);

        if (!string.IsNullOrEmpty(payload.Message))
            builder.AppendLine().AppendLine("Message:").AppendLine(payload.Message);

        if (payload.Headers.Count > 0)
        {
            builder.AppendLine().AppendLine("Headers:");
            foreach (var (key, value) in payload.Headers)
                builder.AppendFormat("- {0}: {1}", key, value).AppendLine();
        }

        return builder.ToString();
    }
}
