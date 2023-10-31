using AutoMapper;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetUserStatisticsOfEmote : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public GetUserStatisticsOfEmote(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (EmoteStatsUserListParams)Parameters[0]!;

        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = await repository.Emote.GetUserStatisticsOfEmoteAsync(parameters, parameters.Pagination);
        var result = await PaginatedResponse<EmoteStatsUserListItem>.CopyAndMapAsync(
            statistics,
            entity => Task.FromResult(Mapper.Map<EmoteStatsUserListItem>(entity))
        );

        return ApiResult.Ok(result);
    }
}
