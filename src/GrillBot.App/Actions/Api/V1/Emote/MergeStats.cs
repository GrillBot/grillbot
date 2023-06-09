using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Core.Managers.Discord;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class MergeStats : ApiAction
{
    private IEmoteManager EmoteManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public MergeStats(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IEmoteManager emoteManager, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        EmoteManager = emoteManager;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task<int> ProcessAsync(MergeEmoteStatsParams parameters)
    {
        await ValidateMergeAsync(parameters);
        await using var repository = DatabaseBuilder.CreateRepository();

        var sourceStats = await repository.Emote.FindStatisticsByEmoteIdAsync(parameters.SourceEmoteId);
        if (sourceStats.Count == 0) return 0;

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

            destinationStatItem.UseCount += item.UseCount;
            repository.Remove(item);
        }

        await WriteToAuditLogAsync(parameters, sourceStats.Count, destinationStats.Count);
        return await repository.CommitAsync();
    }

    private async Task ValidateMergeAsync(MergeEmoteStatsParams @params)
    {
        var supportedEmotes = (await EmoteManager.GetSupportedEmotesAsync()).ConvertAll(o => o.ToString());
        if (!supportedEmotes.Contains(@params.DestinationEmoteId))
        {
            throw new ValidationException(
                new ValidationResult("Nelze sloučit statistiku do neexistujícího emotu.", new[] { nameof(@params.DestinationEmoteId) }), null, @params.DestinationEmoteId
            );
        }
    }

    private async Task WriteToAuditLogAsync(MergeEmoteStatsParams parameters, int sourceStatsCount, int destinationStatsCount)
    {
        var logRequest = new LogRequest
        {
            UserId = ApiContext.GetUserId().ToString(),
            Type = LogType.Info,
            CreatedAt = DateTime.UtcNow,
            LogMessage = new LogMessageRequest
            {
                Message = $"Provedeno sloučení emotů {parameters.SourceEmoteId} do {parameters.DestinationEmoteId}. Sloučeno záznamů: {sourceStatsCount}/{destinationStatsCount}",
                Severity = LogSeverity.Info
            }
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
