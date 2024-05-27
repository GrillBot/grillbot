using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.Emote;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class RemoveStats : ApiAction
{
    private readonly IRabbitMQPublisher _rabbitPublisher;
    private readonly IEmoteServiceClient _emoteServiceClient;

    public RemoveStats(ApiRequestContext apiContext, IRabbitMQPublisher rabbitPublisher, IEmoteServiceClient emoteServiceClient) : base(apiContext)
    {
        _rabbitPublisher = rabbitPublisher;
        _emoteServiceClient = emoteServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var emoteId = (string)Parameters[0]!;
        var guildId = (string)Parameters[1]!;

        var deletedRows = await _emoteServiceClient.DeleteStatisticsAsync(guildId, emoteId);

        await WriteToAuditlogAsync(emoteId, deletedRows);
        return ApiResult.Ok(deletedRows);
    }

    private async Task WriteToAuditlogAsync(string emoteId, int deletedRows)
    {
        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, null, userId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"Statistiky emotu {emoteId} byly smazány. Smazáno záznamů: {deletedRows}",
                Severity = LogSeverity.Info,
                Source = $"Emote.{nameof(RemoveStats)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(logRequest), new());
    }
}
