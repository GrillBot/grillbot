using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Actions.Api.V2.AuditLog;

public class CreateAuditLogMessageAction : ApiAction
{
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public CreateAuditLogMessageAction(ApiRequestContext apiContext, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (Data.Models.API.AuditLog.Public.LogMessageRequest)Parameters[0]!;

        var logType = request.Type.ToLower() switch
        {
            "warning" => LogType.Warning,
            "info" => LogType.Info,
            "error" => LogType.Error,
            _ => throw new NotSupportedException()
        };

        var logSeverity = request.Type.ToLower() switch
        {
            "warning" => LogSeverity.Warning,
            "info" => LogSeverity.Info,
            "error" => LogSeverity.Error,
            _ => throw new NotSupportedException()
        };

        var logRequest = new LogRequest(logType, DateTime.UtcNow, request.GuildId, request.UserId, request.ChannelId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = request.Message,
                Severity = logSeverity,
                Source = request.MessageSource,
                SourceAppName = ApiContext.GetUsername()!
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }), new());
        return ApiResult.Ok();
    }
}
