using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Response;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class MergeStats : ApiAction
{
    private readonly IRabbitMQPublisher _rabbitPublisher;
    private readonly IEmoteServiceClient _emoteServiceClient;

    public MergeStats(ApiRequestContext apiContext, IRabbitMQPublisher rabbitPublisher, IEmoteServiceClient emoteServiceClient) : base(apiContext)
    {
        _rabbitPublisher = rabbitPublisher;
        _emoteServiceClient = emoteServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (MergeEmoteStatsParams)Parameters[0]!;

        var result = await _emoteServiceClient.MergeStatisticsAsync(parameters.GuildId, parameters.SourceEmoteId, parameters.DestinationEmoteId);
        await WriteToAuditLogAsync(parameters, result);

        return ApiResult.Ok(result.ModifiedEmotesCount);
    }

    private async Task WriteToAuditLogAsync(MergeEmoteStatsParams parameters, MergeStatisticsResult result)
    {
        var userId = ApiContext.GetUserId().ToString();
        var message = $"Provedeno sloučení emotů {parameters.SourceEmoteId} do {parameters.DestinationEmoteId}. Vytvořeno: {result.CreatedEmotesCount}. Smazáno: {result.DeletedEmotesCount}. Celkem: {result.ModifiedEmotesCount}";
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, null, userId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = message,
                Severity = LogSeverity.Info,
                Source = $"Emote.{nameof(MergeStats)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }), new());
    }
}
