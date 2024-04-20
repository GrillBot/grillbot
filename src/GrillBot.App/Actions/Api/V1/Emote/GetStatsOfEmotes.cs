using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Extensions;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Core.Services.Emote.Models.Response;
using GrillBot.Data.Extensions.Services;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetStatsOfEmotes : ApiAction
{
    private readonly IEmoteServiceClient _emoteServiceClient;
    private readonly DataResolveManager _dataResolve;

    public GetStatsOfEmotes(ApiRequestContext apiContext, IEmoteServiceClient emoteServiceClient, DataResolveManager dataResolve) : base(apiContext)
    {
        _emoteServiceClient = emoteServiceClient;
        _dataResolve = dataResolve;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (EmotesListParams)Parameters[0]!;
        var unsupported = (bool)Parameters[1]!;

        var result = await ProcessAsync(parameters, unsupported);

        return ApiResult.Ok(result);
    }

    public async Task<PaginatedResponse<GuildEmoteStatItem>> ProcessAsync(EmotesListParams parameters, bool unsupported)
    {
        var request = new EmoteStatisticsListRequest
        {
            EmoteName = parameters.EmoteName,
            FirstOccurenceFrom = (parameters.FirstOccurence?.From).ConvertKindToUtc(DateTimeKind.Local),
            FirstOccurenceTo = (parameters.FirstOccurence?.To).ConvertKindToUtc(DateTimeKind.Local),
            GuildId = parameters.GuildId,
            IgnoreAnimated = parameters.FilterAnimated,
            LastOccurenceFrom = (parameters.LastOccurence?.From).ConvertKindToUtc(DateTimeKind.Local),
            LastOccurenceTo = (parameters.LastOccurence?.To).ConvertKindToUtc(DateTimeKind.Local),
            Pagination = parameters.Pagination,
            Sort = parameters.Sort,
            Unsupported = unsupported,
            UseCountFrom = parameters.UseCount?.From,
            UseCountTo = parameters.UseCount?.To,
            UserId = parameters.UserId
        };

        var data = await _emoteServiceClient.GetEmoteStatisticsListAsync(request);
        return await PaginatedResponse<GuildEmoteStatItem>.CopyAndMapAsync(data, MapItemAsync);
    }

    private async Task<GuildEmoteStatItem> MapItemAsync(EmoteStatisticsItem item)
    {
        var guild = await _dataResolve.GetGuildAsync(item.GuildId.ToUlong());

        return new GuildEmoteStatItem
        {
            Emote = item.ToEmoteItem(),
            FirstOccurence = item.FirstOccurence.WithKind(DateTimeKind.Utc).ToLocalTime(),
            Guild = guild!,
            LastOccurence = item.LastOccurence.WithKind(DateTimeKind.Utc).ToLocalTime(),
            UseCount = item.UseCount,
            UsedUsersCount = item.UsersCount
        };
    }
}
