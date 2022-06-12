using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Models;

namespace GrillBot.App.Services.Emotes;

public class EmotesApiService : ServiceBase
{
    private EmotesCacheService EmotesCacheService { get; }

    public EmotesApiService(GrillBotDatabaseBuilder dbFactory, EmotesCacheService emotesCacheService,
        IMapper mapper) : base(null, dbFactory, mapper: mapper)
    {
        EmotesCacheService = emotesCacheService;
    }

    public async Task<PaginatedResponse<EmoteStatItem>> GetStatsOfEmotesAsync(EmotesListParams @params, bool unsupported)
    {
        var supportedEmotes = EmotesCacheService.GetSupportedEmotes()
            .ConvertAll(o => o.Item1.ToString());

        using var context = CreateContext();

        var query = context.CreateQuery(@params, true);

        if (unsupported)
            query = query.Where(o => !supportedEmotes.Contains(o.EmoteId));
        else
            query = query.Where(o => supportedEmotes.Contains(o.EmoteId));

        var groupedQuery = query.GroupBy(o => o.EmoteId).Select(o => new Data.Models.EmoteStatItem()
        {
            Id = o.Key,
            LastOccurence = o.Max(x => x.LastOccurence),
            FirstOccurence = o.Min(x => x.FirstOccurence),
            UseCount = o.Sum(x => x.UseCount),
            UsersCount = o.Count()
        });

        groupedQuery = GetFilterAndSortQuery(groupedQuery, @params);

        var result = await PaginatedResponse<EmoteStatItem>
            .CreateAsync(groupedQuery, @params.Pagination, entity => Mapper.Map<EmoteStatItem>(entity));

        if (unsupported)
            result.Data.ForEach(o => o.Emote.ImageUrl = null);

        return result;
    }

    private static IQueryable<Data.Models.EmoteStatItem> GetFilterAndSortQuery(IQueryable<Data.Models.EmoteStatItem> query, EmotesListParams @params)
    {
        if (@params.UseCount != null)
        {
            if (@params.UseCount.From != null)
                query = query.Where(o => o.UseCount >= @params.UseCount.From.Value);

            if (@params.UseCount.To != null)
                query = query.Where(o => o.UseCount < @params.UseCount.To.Value);
        }

        if (@params.FirstOccurence != null)
        {
            if (@params.FirstOccurence.From != null)
                query = query.Where(o => o.FirstOccurence >= @params.FirstOccurence.From.Value);

            if (@params.FirstOccurence.To != null)
                query = query.Where(o => o.FirstOccurence < @params.FirstOccurence.To.Value);
        }

        if (@params.LastOccurence != null)
        {
            if (@params.LastOccurence.From != null)
                query = query.Where(o => o.LastOccurence >= @params.LastOccurence.From.Value);

            if (@params.LastOccurence.To != null)
                query = query.Where(o => o.LastOccurence < @params.LastOccurence.To.Value);
        }

        return @params.Sort.OrderBy switch
        {
            "UseCount" => @params.Sort.Descending switch
            {
                true => query.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.Id).ThenByDescending(o => o.LastOccurence),
                _ => query.OrderBy(o => o.UseCount).ThenBy(o => o.Id).ThenBy(o => o.LastOccurence)
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
                true => query.OrderByDescending(o => o.Id).ThenByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccurence),
                _ => query.OrderBy(o => o.Id).ThenBy(o => o.UseCount).ThenBy(o => o.LastOccurence)
            }
        };
    }

    public async Task<int> MergeStatsToAnotherAsync(MergeEmoteStatsParams @params)
    {
        ValidateMerge(@params);

        using var context = CreateContext();

        var sourceStats = await context.Emotes.AsQueryable()
            .Where(o => o.EmoteId == @params.SourceEmoteId)
            .ToListAsync();

        if (sourceStats.Count == 0)
            return 0;

        var destinationStats = await context.Emotes.AsQueryable()
            .Where(o => o.EmoteId == @params.DestinationEmoteId)
            .ToListAsync();

        foreach (var item in sourceStats)
        {
            var destinationStatItem = destinationStats.Find(o => o.UserId == item.UserId && o.GuildId == item.GuildId);

            if (destinationStatItem == null)
            {
                destinationStatItem = new Database.Entity.EmoteStatisticItem()
                {
                    EmoteId = item.EmoteId,
                    GuildId = item.GuildId,
                    UserId = item.UserId
                };

                await context.AddAsync(destinationStatItem);
            }

            if (item.LastOccurence > destinationStatItem.LastOccurence)
                destinationStatItem.LastOccurence = item.LastOccurence;

            if (item.FirstOccurence != DateTime.MinValue && (item.FirstOccurence < destinationStatItem.FirstOccurence || destinationStatItem.FirstOccurence == DateTime.MinValue))
                destinationStatItem.FirstOccurence = item.FirstOccurence;

            destinationStatItem.UseCount += item.UseCount;
            context.Remove(item);
        }

        // TODO Log merge to AuditLog.
        return await context.SaveChangesAsync();
    }

    private void ValidateMerge(MergeEmoteStatsParams @params)
    {
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
        using var context = CreateContext();

        var emotes = await context.Emotes
            .Where(o => o.EmoteId == emoteId)
            .ToListAsync();

        if (emotes.Count == 0)
            return 0;

        // TODO Log remove to AuditLog.
        context.RemoveRange(emotes);
        return await context.SaveChangesAsync();
    }
}
