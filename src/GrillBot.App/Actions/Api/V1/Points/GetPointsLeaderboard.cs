﻿using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Core.Services.PointsService.Enums;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetPointsLeaderboard : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    private readonly DataResolveManager _dataResolveManager;
    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;

    public GetPointsLeaderboard(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        IServiceClientExecutor<IPointsServiceClient> pointsServiceClient, DataResolveManager dataResolveManager) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        _pointsServiceClient = pointsServiceClient;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var result = new List<UserPointsItem>();
        using var repository = DatabaseBuilder.CreateRepository();

        const LeaderboardColumnFlag leaderboardColumns = LeaderboardColumnFlag.YearBack | LeaderboardColumnFlag.MonthBack | LeaderboardColumnFlag.Today | LeaderboardColumnFlag.Total;
        const LeaderboardSortOptions leaderboardSort = LeaderboardSortOptions.ByTotalDescending;

        var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId());
        foreach (var guildId in mutualGuilds.Select(o => o.Id))
        {
            var leaderboard = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.GetLeaderboardAsync(guildId.ToString(), 0, 0, leaderboardColumns, leaderboardSort, ctx.CancellationToken));
            var guildData = (await _dataResolveManager.GetGuildAsync(guildId))!;
            var nicknames = await repository.GuildUser.GetUserNicknamesAsync(guildId);

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
