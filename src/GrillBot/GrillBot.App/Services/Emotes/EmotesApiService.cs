using AutoMapper;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Emotes;

public class EmotesApiService
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private EmotesCacheService EmotesCacheService { get; }
    private IMapper Mapper { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public EmotesApiService(GrillBotDatabaseBuilder databaseBuilder, EmotesCacheService emotesCacheService,
        IMapper mapper, ApiRequestContext apiRequestContext, AuditLogWriter auditLogWriter)
    {
        EmotesCacheService = emotesCacheService;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        ApiRequestContext = apiRequestContext;
        AuditLogWriter = auditLogWriter;
    }

    public async Task<PaginatedResponse<EmoteStatItem>> GetStatsOfEmotesAsync(EmotesListParams @params, bool unsupported)
    {
        var supportedEmotes = EmotesCacheService.GetSupportedEmotes()
            .ConvertAll(o => o.Item1.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();
        var statisticsData = await repository.Emote.GetEmoteStatisticsDataAsync(@params, supportedEmotes, unsupported);

        var statsData = SetStatsFilter(statisticsData, @params);
        statsData = SetStatsSort(statsData, @params);

        var stats = statsData
            .Select(o => Mapper.Map<EmoteStatItem>(o))
            .ToList();

        var result = PaginatedResponse<EmoteStatItem>.Create(stats, @params.Pagination);
        if (unsupported)
            result.Data.ForEach(o => o.Emote.ImageUrl = null);

        return result;
    }

    private static IEnumerable<Database.Models.Emotes.EmoteStatItem> SetStatsFilter(IEnumerable<Database.Models.Emotes.EmoteStatItem> data,
        EmotesListParams @params)
    {
        if (@params.UseCount != null)
        {
            if (@params.UseCount.From != null)
                data = data.Where(o => o.UseCount >= @params.UseCount.From.Value);

            if (@params.UseCount.To != null)
                data = data.Where(o => o.UseCount < @params.UseCount.To.Value);
        }

        if (@params.FirstOccurence != null)
        {
            if (@params.FirstOccurence.From != null)
                data = data.Where(o => o.FirstOccurence >= @params.FirstOccurence.From.Value);

            if (@params.FirstOccurence.To != null)
                data = data.Where(o => o.FirstOccurence < @params.FirstOccurence.To.Value);
        }

        if (@params.LastOccurence != null)
        {
            if (@params.LastOccurence.From != null)
                data = data.Where(o => o.LastOccurence >= @params.LastOccurence.From.Value);

            if (@params.LastOccurence.To != null)
                data = data.Where(o => o.LastOccurence < @params.LastOccurence.To.Value);
        }

        return data;
    }

    private static IEnumerable<Database.Models.Emotes.EmoteStatItem> SetStatsSort(IEnumerable<Database.Models.Emotes.EmoteStatItem> query,
        EmotesListParams @params)
    {
        return @params.Sort.OrderBy switch
        {
            "UseCount" => @params.Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.EmoteId).ThenByDescending(o => o.LastOccurence),
                _ => query.OrderBy(o => o.UseCount).ThenBy(o => o.EmoteId).ThenBy(o => o.LastOccurence)
            },
            "FirstOccurence" => @params.Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.FirstOccurence),
                _ => query.OrderBy(o => o.FirstOccurence)
            },
            "LastOccurence" => @params.Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.FirstOccurence),
                _ => query.OrderBy(o => o.FirstOccurence)
            },
            _ => @params.Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.EmoteId).ThenByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccurence),
                _ => query.OrderBy(o => o.EmoteId).ThenBy(o => o.UseCount).ThenBy(o => o.LastOccurence)
            }
        };
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
