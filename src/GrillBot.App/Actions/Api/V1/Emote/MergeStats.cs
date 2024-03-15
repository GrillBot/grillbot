using GrillBot.Common.Extensions;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Discord;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class MergeStats : ApiAction
{
    private IEmoteManager EmoteManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public MergeStats(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IEmoteManager emoteManager, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        EmoteManager = emoteManager;
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (MergeEmoteStatsParams)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();
        await ValidateMergeAsync(repository, parameters);

        var sourceStats = await repository.Emote.FindStatisticsByEmoteIdAsync(parameters.SourceEmoteId);
        if (sourceStats.Count == 0)
            return ApiResult.Ok(0);

        var destinationStats = await repository.Emote.FindStatisticsByEmoteIdAsync(parameters.DestinationEmoteId);
        foreach (var item in sourceStats)
        {
            var destinationStatItem = destinationStats.Find(o => o.UserId == item.UserId && o.GuildId == item.GuildId);
            if (destinationStatItem == null)
            {
                destinationStatItem = new Database.Entity.EmoteStatisticItem
                {
                    EmoteId = parameters.DestinationEmoteId,
                    GuildId = item.GuildId,
                    UserId = item.UserId
                };

                await repository.AddAsync(destinationStatItem);
            }

            if (item.LastOccurence > destinationStatItem.LastOccurence)
                destinationStatItem.LastOccurence = item.LastOccurence;

            if (item.FirstOccurence != DateTime.MinValue && (item.FirstOccurence < destinationStatItem.FirstOccurence || destinationStatItem.FirstOccurence == DateTime.MinValue))
                destinationStatItem.FirstOccurence = item.FirstOccurence;

            destinationStatItem.IsEmoteSupported = true;
            destinationStatItem.UseCount += item.UseCount;
            repository.Remove(item);
        }

        await WriteToAuditLogAsync(parameters, sourceStats.Count, destinationStats.Count);
        var result = await repository.CommitAsync();

        return ApiResult.Ok(result);
    }

    private static async Task ValidateMergeAsync(GrillBotRepository repository, MergeEmoteStatsParams @params)
    {
        if (!await repository.Emote.IsEmoteSupportedAsync(@params.DestinationEmoteId))
            throw new ValidationException("Nelze sloučit statistiku do neexistujícího emotu.").ToBadRequestValidation(null, @params.DestinationEmoteId);

        if (await repository.Emote.IsEmoteSupportedAsync(@params.SourceEmoteId))
            throw new ValidationException("Nelze slučovat statistiku zatím existujícího emotu.").ToBadRequestValidation(null, @params.SourceEmoteId);
    }

    private async Task WriteToAuditLogAsync(MergeEmoteStatsParams parameters, int sourceStatsCount, int destinationStatsCount)
    {
        var userId = ApiContext.GetUserId().ToString();
        var message = $"Provedeno sloučení emotů {parameters.SourceEmoteId} do {parameters.DestinationEmoteId}. Sloučeno záznamů: {sourceStatsCount}/{destinationStatsCount}";
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

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
    }
}
