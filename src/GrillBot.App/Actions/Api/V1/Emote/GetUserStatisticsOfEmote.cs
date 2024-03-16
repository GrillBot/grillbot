using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Core.Services.Emote.Models.Response;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetUserStatisticsOfEmote : ApiAction
{
    private readonly IEmoteServiceClient _emoteServiceClient;
    private readonly DataResolveManager _dataResolve;

    public GetUserStatisticsOfEmote(ApiRequestContext apiContext, IEmoteServiceClient emoteServiceClient, DataResolveManager dataResolve) : base(apiContext)
    {
        _emoteServiceClient = emoteServiceClient;
        _dataResolve = dataResolve;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (EmoteStatsUserListParams)Parameters[0]!;

        var request = new EmoteUserUsageListRequest
        {
            EmoteId = parameters.EmoteId,
            GuildId = parameters.GuildId,
            Pagination = parameters.Pagination,
            Sort = parameters.Sort
        };

        var response = await _emoteServiceClient.GetUserEmoteUsageListAsync(request);
        var result = await PaginatedResponse<EmoteStatsUserListItem>.CopyAndMapAsync(response, entity => MapItemAsync(entity, parameters.GuildId));
        return ApiResult.Ok(result);
    }

    private async Task<EmoteStatsUserListItem> MapItemAsync(EmoteUserUsageItem item, string guildId)
    {
        var guild = await _dataResolve.GetGuildAsync(guildId.ToUlong());
        var user = await _dataResolve.GetUserAsync(item.UserId.ToUlong());

        return new EmoteStatsUserListItem
        {
            FirstOccurence = item.FirstOccurence.WithKind(DateTimeKind.Utc).ToLocalTime(),
            Guild = guild!,
            LastOccurence = item.LastOccurence.WithKind(DateTimeKind.Utc).ToLocalTime(),
            UseCount = item.UseCount,
            User = user!
        };
    }
}
