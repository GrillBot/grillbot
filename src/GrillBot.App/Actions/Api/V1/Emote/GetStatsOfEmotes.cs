using AutoMapper;
using GrillBot.App.Helpers;
using GrillBot.Common.Models;
using GrillBot.Common.Models.Pagination;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetStatsOfEmotes : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private EmoteHelper EmoteHelper { get; }

    public GetStatsOfEmotes(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper, EmoteHelper emoteHelper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        EmoteHelper = emoteHelper;
    }

    public async Task<PaginatedResponse<EmoteStatItem>> ProcessAsync(EmotesListParams parameters, bool unsupported)
    {
        var supportedEmotes = (await EmoteHelper.GetSupportedEmotesAsync()).ConvertAll(o => o.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.Emote.GetEmoteStatisticsDataAsync(parameters, supportedEmotes, unsupported);
        var filtered = SetStatsFilter(data, parameters);
        filtered = SetStatsSort(filtered, parameters);

        var stats = filtered.Select(o => Mapper.Map<EmoteStatItem>(o)).ToList();
        var result = PaginatedResponse<EmoteStatItem>.Create(stats, parameters.Pagination);
        if (unsupported)
            result.Data.ForEach(o => o.Emote.ImageUrl = null);

        return result;
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
