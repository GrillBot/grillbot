using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Emote;
using GrillBot.Data.Models.API.Emotes;

namespace GrillBot.App.Actions.Api.V1.Emote;

public class MergeStats : ApiAction
{
    private readonly IEmoteServiceClient _emoteServiceClient;

    public MergeStats(ApiRequestContext apiContext, IEmoteServiceClient emoteServiceClient) : base(apiContext)
    {
        _emoteServiceClient = emoteServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (MergeEmoteStatsParams)Parameters[0]!;

        var result = await _emoteServiceClient.MergeStatisticsAsync(parameters.GuildId, parameters.SourceEmoteId, parameters.DestinationEmoteId);
        return ApiResult.Ok(result.ModifiedEmotesCount);
    }
}
