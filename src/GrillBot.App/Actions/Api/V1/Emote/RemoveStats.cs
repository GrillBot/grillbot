using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class RemoveStats : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public RemoveStats(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var emoteId = (string)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var emotes = await repository.Emote.FindStatisticsByEmoteIdAsync(emoteId);
        if (emotes.Count == 0)
            return ApiResult.Ok(0);

        await WriteToAuditlogAsync(emoteId, emotes.Count);
        repository.RemoveCollection(emotes);
        var result = await repository.CommitAsync();

        return ApiResult.Ok(result);
    }

    private async Task WriteToAuditlogAsync(string emoteId, int emotesCount)
    {
        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, null, userId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"Statistiky emotu {emoteId} byly smazány. Smazáno záznamů: {emotesCount}",
                Severity = LogSeverity.Info,
                Source = $"Emote.{nameof(RemoveStats)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
    }
}
