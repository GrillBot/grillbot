using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Data.Models.API.Points;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetTransactionList : ApiAction
{
    private IPointsServiceClient PointsServiceClient { get; }

    private readonly DataResolveManager _dataResolveManager;

    public GetTransactionList(ApiRequestContext apiContext, IPointsServiceClient pointsServiceClient, DataResolveManager dataResolveManager) : base(apiContext)
    {
        PointsServiceClient = pointsServiceClient;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (AdminListRequest)Parameters[0]!;

        var transactions = await PointsServiceClient.GetTransactionListAsync(request);
        var result = await PaginatedResponse<PointsTransaction>.CopyAndMapAsync(transactions, async entity =>
        {
            var guild = await _dataResolveManager.GetGuildAsync(entity.GuildId.ToUlong());
            var user = await _dataResolveManager.GetUserAsync(entity.UserId.ToUlong());

            var mergeInfo = entity.MergedCount > 0
                ? new PointsMergeInfo
                {
                    MergedItemsCount = entity.MergedCount,
                    MergeRangeFrom = entity.MergedFrom.GetValueOrDefault(),
                    MergeRangeTo = entity.MergedTo.GetValueOrDefault()
                }
                : null;

            if (mergeInfo != null && mergeInfo.MergeRangeFrom == mergeInfo.MergeRangeTo)
                mergeInfo.MergeRangeTo = null;

            return new PointsTransaction
            {
                Points = entity.Value,
                CreatedAt = entity.CreatedAt.ToLocalTime(),
                ReactionId = entity.ReactionId,
                MessageId = entity.MessageId,
                MergeInfo = mergeInfo,
                Guild = guild!,
                User = user!
            };
        });

        return ApiResult.Ok(result);
    }
}
