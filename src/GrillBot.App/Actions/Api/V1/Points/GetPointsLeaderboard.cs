using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Core.Services.PointsService.Enums;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.UserManagementService;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetPointsLeaderboard(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient,
    DataResolveManager _dataResolveManager,
    IServiceClientExecutor<IUserManagementServiceClient> _userManagementClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var result = new List<UserPointsItem>();

        const LeaderboardColumnFlag leaderboardColumns = LeaderboardColumnFlag.YearBack | LeaderboardColumnFlag.MonthBack | LeaderboardColumnFlag.Today | LeaderboardColumnFlag.Total;
        const LeaderboardSortOptions leaderboardSort = LeaderboardSortOptions.ByTotalDescending;

        var mutualGuilds = await _discordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        foreach (var guildId in mutualGuilds.Select(o => o.Id))
        {
            var leaderboard = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.GetLeaderboardAsync(guildId.ToString(), 0, 0, leaderboardColumns, leaderboardSort, ctx.CancellationToken));
            var guildData = (await _dataResolveManager.GetGuildAsync(guildId))!;
            var nicknames = await _userManagementClient.ExecuteRequestAsync((c, ctx) => c.GetGuildUsersWithNicknameAsync(guildId, ctx.CancellationToken));

            foreach (var item in leaderboard)
            {
                var user = await _dataResolveManager.GetUserAsync(item.UserId.ToUlong());
                if (user is null)
                    continue;

                result.Add(new UserPointsItem
                {
                    Guild = guildData,
                    Nickname = nicknames.TryGetValue(item.UserId, out var nickname) ? nickname : null,
                    User = user,
                    PointsToday = item.Today,
                    TotalPoints = item.Total,
                    PointsMonthBack = item.MonthBack,
                    PointsYearBack = item.YearBack
                });
            }
        }

        result = [.. result.OrderByDescending(o => o.PointsYearBack)];
        return ApiResult.Ok(result);
    }
}
