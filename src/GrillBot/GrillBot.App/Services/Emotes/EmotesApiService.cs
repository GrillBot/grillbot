using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.Emotes;

public class EmotesApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private EmotesCacheService EmotesCacheService { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public EmotesApiService(GrillBotDatabaseBuilder databaseBuilder, EmotesCacheService emotesCacheService, ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        EmotesCacheService = emotesCacheService;
        DatabaseBuilder = databaseBuilder;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
    }

    public async Task<int> MergeStatsToAnotherAsync(MergeEmoteStatsParams @params)
    {
        ValidateMerge(@params);

        await using var repository = DatabaseBuilder.CreateRepository();

        var sourceStats = await repository.Emote.FindStatisticsByEmoteIdAsync(@params.SourceEmoteId);
        if (sourceStats.Count == 0)
            return 0;

        var destinationStats = await repository.Emote.FindStatisticsByEmoteIdAsync(@params.DestinationEmoteId);
        foreach (var item in sourceStats)
        {
            var destinationStatItem = destinationStats.Find(o => o.UserId == item.UserId && o.GuildId == item.GuildId);
            if (destinationStatItem == null)
            {
                destinationStatItem = new Database.Entity.EmoteStatisticItem
                {
                    EmoteId = @params.DestinationEmoteId,
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

        var logItem = new AuditLogDataWrapper(AuditLogItemType.Info,
            $"Provedeno sloučení emotů {@params.SourceEmoteId} do {@params.DestinationEmoteId}. Sloučeno záznamů: {sourceStats.Count}/{destinationStats.Count}",
            null, null, ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(logItem);
        return await repository.CommitAsync();
    }

    private void ValidateMerge(MergeEmoteStatsParams @params)
    {
        if (@params.SuppressValidations) return;
        var supportedEmotes = EmotesCacheService.GetSupportedEmotes().ConvertAll(o => o.Item1.ToString());

        if (!supportedEmotes.Contains(@params.DestinationEmoteId))
        {
            throw new ValidationException(
                new ValidationResult("Nelze sloučit statistiku do neexistujícího emotu.", new[] { nameof(@params.DestinationEmoteId) }), null, @params.DestinationEmoteId
            );
        }
    }

    public async Task<int> RemoveStatisticsAsync(string emoteId)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var emotes = await repository.Emote.FindStatisticsByEmoteIdAsync(emoteId);

        if (emotes.Count == 0)
            return 0;

        var auditLogItem = new AuditLogDataWrapper(AuditLogItemType.Info,
            $"Statistiky emotu {emoteId} byly smazány. Smazáno záznamů: {emotes.Count}", null, null,
            ApiRequestContext.LoggedUser);
        await AuditLogWriter.StoreAsync(auditLogItem);

        repository.RemoveCollection(emotes);
        return await repository.CommitAsync();
    }
}
