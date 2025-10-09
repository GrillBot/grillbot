using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using PointsService;
using PointsService.Models;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums.Internal;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.App.Managers.DataResolve;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ComputeUserPoints : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    private readonly DataResolveManager _dataResolveManager;
    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;

    public ComputeUserPoints(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient,
        IServiceClientExecutor<IPointsServiceClient> pointsServiceClient, DataResolveManager dataResolveManager) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        _pointsServiceClient = pointsServiceClient;
        _dataResolveManager = dataResolveManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var userId = Parameters.OfType<ulong?>().FirstOrDefault();
        userId ??= ApiContext.GetUserId();

        var result = new List<UserPointsItem>();
        foreach (var guildId in await GetGuildIdsAsync(userId.Value))
        {
            var status = await _pointsServiceClient.ExecuteRequestAsync((c, ctx) => c.GetStatusOfPointsAsync(guildId.ToString(), userId.Value.ToString(), ctx.CancellationToken));
            result.Add(await TransformStatusAsync(guildId, userId.Value, status));
        }

        return ApiResult.Ok(result);
    }

    private async Task<UserPointsItem> TransformStatusAsync(ulong guildId, ulong userId, PointsStatus status)
    {
        var guild = await _dataResolveManager.GetGuildAsync(guildId);
        var user = await _dataResolveManager.GetUserAsync(userId);

        return new UserPointsItem
        {
            Guild = guild!,
            User = user!,
            PointsToday = status.Today,
            TotalPoints = status.Total,
            PointsMonthBack = status.MonthBack,
            PointsYearBack = status.YearBack
        };
    }

    private async Task<List<ulong>> GetGuildIdsAsync(ulong userId)
    {
        var ids = new List<ulong>();
        ids.AddRange((await DiscordClient.FindMutualGuildsAsync(userId)).ConvertAll(o => o.Id));

        if (ApiContext.IsPublic())
            return ids;

        using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.User.FindUserByIdAsync(userId, UserIncludeOptions.Guilds, true);
        if (user != null)
            ids.AddRange(user.Guilds.Select(o => o.GuildId.ToUlong()));

        return ids.Distinct().ToList();
    }
}
