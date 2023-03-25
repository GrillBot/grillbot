using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums.Internal;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Points;

public class ComputeUserPoints : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public ComputeUserPoints(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, IMapper mapper,
        IPointsServiceClient pointsServiceClient) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Mapper = mapper;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<List<UserPointsItem>> ProcessAsync(ulong? userId)
    {
        userId ??= ApiContext.GetUserId();

        var result = new List<UserPointsItem>();
        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var guildId in await GetGuildIdsAsync(userId.Value))
        {
            var status = await PointsServiceClient.GetStatusOfPointsAsync(guildId.ToString(), userId.Value.ToString());
            result.Add(await TransformStatusAsync(repository, guildId, userId.Value, status));
        }

        return result;
    }

    private async Task<UserPointsItem> TransformStatusAsync(GrillBotRepository repository, ulong guildId, ulong userId, PointsStatus status)
    {
        return new UserPointsItem
        {
            Guild = await TransformGuildAsync(repository, guildId),
            User = await TransformUserAsync(repository, userId),

            PointsToday = status.Today,
            TotalPoints = status.Total,
            PointsMonthBack = status.MonthBack,
            PointsYearBack = status.YearBack
        };
    }

    private async Task<Data.Models.API.Guilds.Guild> TransformGuildAsync(GrillBotRepository repository, ulong guildId)
    {
        var dbGuild = (await repository.Guild.FindGuildByIdAsync(guildId, true))!;
        return Mapper.Map<Data.Models.API.Guilds.Guild>(dbGuild);
    }

    private async Task<Data.Models.API.Users.User> TransformUserAsync(GrillBotRepository repository, ulong userId)
    {
        var user = await repository.User.FindUserByIdAsync(userId, UserIncludeOptions.None, true);
        return Mapper.Map<Data.Models.API.Users.User>(user);
    }

    private async Task<List<ulong>> GetGuildIdsAsync(ulong userId)
    {
        var ids = new List<ulong>();
        ids.AddRange((await DiscordClient.FindMutualGuildsAsync(userId)).ConvertAll(o => o.Id));

        if (ApiContext.IsPublic())
            return ids;

        await using var repository = DatabaseBuilder.CreateRepository();

        var user = await repository.User.FindUserByIdAsync(userId, UserIncludeOptions.Guilds, true);
        if (user != null)
            ids.AddRange(user.Guilds.Select(o => o.GuildId.ToUlong()));

        return ids.Distinct().ToList();
    }
}
