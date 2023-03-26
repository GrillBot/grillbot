using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Common.Services.PointsService;
using GrillBot.Core.Extensions;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.App.Actions.Api.V1.Points;

public class GetPointsLeaderboard : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public GetPointsLeaderboard(ApiRequestContext apiContext, IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper,
        IPointsServiceClient pointsServiceClient) : base(apiContext)
    {
        DiscordClient = discordClient;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<List<UserPointsItem>> ProcessAsync()
    {
        var result = new List<UserPointsItem>();
        await using var repository = DatabaseBuilder.CreateRepository();

        foreach (var guild in await DiscordClient.FindMutualGuildsAsync(ApiContext.GetUserId()))
        {
            var leaderboard = await PointsServiceClient.GetLeaderboardAsync(guild.Id.ToString(), 0, 0);
            leaderboard.ValidationErrors.AggregateAndThrow();

            var guildData = Mapper.Map<Data.Models.API.Guilds.Guild>(await repository.Guild.FindGuildAsync(guild, true));

            foreach (var item in leaderboard.Response!.Items)
            {
                var user = await guild.GetUserAsync(item.UserId.ToUlong());
                if (user is null) continue;

                var guildUser = await repository.GuildUser.FindGuildUserAsync(user, true);
                if (guildUser is null) continue;

                result.Add(new UserPointsItem
                {
                    Guild = guildData,
                    Nickname = guildUser.Nickname,
                    User = Mapper.Map<Data.Models.API.Users.User>(guildUser.User),
                    PointsToday = item.Today,
                    TotalPoints = item.Total,
                    PointsMonthBack = item.MonthBack,
                    PointsYearBack = item.YearBack
                });
            }
        }

        return result
            .OrderByDescending(o => o.PointsYearBack)
            .ToList();
    }
}
