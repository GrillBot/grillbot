using GrillBot.App.Managers.DataResolve;
using GrillBot.Common.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models.Users;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetUserList : ApiAction
{
    private IPointsServiceClient PointsServiceClient { get; }
    private readonly DataResolveManager _dataResolveManager;

    public GetUserList(ApiRequestContext apiContext, IPointsServiceClient pointsServiceClient, DataResolveManager dataResolveManager) : base(apiContext)
    {
        PointsServiceClient = pointsServiceClient;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = (UserListRequest)Parameters[0]!;
        var userList = await PointsServiceClient.GetUserListAsync(request);

        var result = await PaginatedResponse<Data.Models.API.Points.UserListItem>.CopyAndMapAsync(userList, MapItemAsync);
        return ApiResult.Ok(result);
    }

    private async Task<Data.Models.API.Points.UserListItem> MapItemAsync(UserListItem item)
    {
        var guild = await _dataResolveManager.GetGuildAsync(item.GuildId.ToUlong());
        var user = await _dataResolveManager.GetUserAsync(item.UserId.ToUlong());

        return new Data.Models.API.Points.UserListItem
        {
            ActivePoints = item.ActivePoints,
            ExpiredPoints = item.ExpiredPoints,
            Guild = guild!,
            User = user!,
            MergedPoints = item.MergedPoints,
            PointsDeactivated = item.PointsDeactivated
        };
    }
}
