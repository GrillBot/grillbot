using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetStatsOfEmotes : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetStatsOfEmotes(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (EmotesListParams)Parameters[0]!;
        var unsupported = (bool)Parameters[0]!;
        var result = await ProcessAsync(parameters, unsupported);

        return ApiResult.Ok(result);
    }

    public async Task<PaginatedResponse<GuildEmoteStatItem>> ProcessAsync(EmotesListParams parameters, bool unsupported)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Emote.GetEmoteStatisticsDataAsync(parameters, unsupported);
        var filtered = SetStatsFilter(data, parameters);
        filtered = SetStatsSort(filtered, parameters);

        var guilds = await repository.Guild.GetGuildsByIdsAsync(
            filtered.Select(o => o.GuildId).Distinct().ToList()
        );
        var stats = filtered.Select(o => MapItem(o, guilds)).ToList();
        var result = PaginatedResponse<GuildEmoteStatItem>.Create(stats, parameters.Pagination);
        if (unsupported)
            result.Data.ForEach(o => o.Emote.ImageUrl = null);

        return result;
    }

    private GuildEmoteStatItem MapItem(Database.Models.Emotes.EmoteStatItem item, List<Database.Entity.Guild> guilds)
    {
        var mapped = Mapper.Map<GuildEmoteStatItem>(item);

        var guild = guilds.First(o => o.Id == item.GuildId);
        mapped.Guild = Mapper.Map<Data.Models.API.Guilds.Guild>(guild);

        return mapped;
    }

    private static IEnumerable<Database.Models.Emotes.EmoteStatItem> SetStatsFilter(IEnumerable<Database.Models.Emotes.EmoteStatItem> data, EmotesListParams @params)
    {
        if (@params.UseCount?.From != null)
            data = data.Where(o => o.UseCount >= @params.UseCount.From.Value);

        if (@params.UseCount?.To != null)
            data = data.Where(o => o.UseCount < @params.UseCount.To.Value);

        if (@params.FirstOccurence?.From != null)
            data = data.Where(o => o.FirstOccurence >= @params.FirstOccurence.From.Value);

        if (@params.FirstOccurence?.To != null)
            data = data.Where(o => o.FirstOccurence < @params.FirstOccurence.To.Value);

        if (@params.LastOccurence?.From != null)
            data = data.Where(o => o.LastOccurence >= @params.LastOccurence.From.Value);

        if (@params.LastOccurence?.To != null)
            data = data.Where(o => o.LastOccurence < @params.LastOccurence.To.Value);

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
}
