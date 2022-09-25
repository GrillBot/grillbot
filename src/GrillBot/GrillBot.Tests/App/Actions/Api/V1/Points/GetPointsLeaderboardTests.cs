using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class GetPointsLeaderboardTests : ApiActionTest<GetPointsLeaderboard>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override GetPointsLeaderboard CreateAction()
    {
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).Build();

        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();
        return new GetPointsLeaderboard(ApiRequestContext, client, DatabaseBuilder, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(100, result[0].TotalPoints);
        Assert.AreEqual(100, result[0].PointsToday);
        Assert.AreEqual(100, result[0].PointsMonthBack);
        Assert.AreEqual(100, result[0].PointsYearBack);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(new Database.Entity.PointsTransactionSummary
        {
            Day = DateTime.Today,
            GuildId = Consts.GuildId.ToString(),
            MessagePoints = 50,
            ReactionPoints = 50,
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();
    }
}
