using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Request;
using GrillBot.Data.Extensions.Services;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class GetStatOfEmote : ApiAction
{
    private readonly IEmoteServiceClient _emoteServiceClient;

    public GetStatOfEmote(ApiRequestContext apiContext, IEmoteServiceClient emoteServiceClient) : base(apiContext)
    {
        _emoteServiceClient = emoteServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (string)Parameters[0]!;
        var emoteId = (string)Parameters[1]!;
        var isUnsupported = (bool)Parameters[2]!;

        var request = new EmoteStatisticsListRequest
        {
            EmoteFullId = emoteId,
            GuildId = guildId,
            Pagination = { PageSize = 1 },
            Unsupported = isUnsupported
        };

        var statistics = await _emoteServiceClient.GetEmoteStatisticsListAsync(request);
        if (statistics.TotalItemsCount == 0)
            throw new NotFoundException();

        var item = statistics.Data[0];
        var result = new EmoteStatItem
        {
            Emote = item.ToEmoteItem(),
            FirstOccurence = item.FirstOccurence.WithKind(DateTimeKind.Utc).ToLocalTime(),
            LastOccurence = item.LastOccurence.WithKind(DateTimeKind.Utc).ToLocalTime(),
            UseCount = item.UseCount,
            UsedUsersCount = item.UsersCount
        };

        return ApiResult.Ok(result);
    }
}
