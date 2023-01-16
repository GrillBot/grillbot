using System.IO;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands.Points.Chart;
using GrillBot.App.Infrastructure.IO;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Points.Chart;

[TestClass]
public class PointsChartTests : CommandActionTest<PointsChart>
{
    private static readonly IRole Role = new RoleBuilder(Consts.RoleId, Consts.RoleName).SetColor(Color.Blue).Build();
    private static readonly GuildBuilder GuildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetRoleAction(Role);

    private static readonly GuildUserBuilder UserBuilder = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(GuildBuilder.Build())
        .SetRoles(new[] { Role.Id });

    private static readonly IGuildUser GuildUser = UserBuilder.Build();

    private static readonly IGuildUser[] AnotherUsers =
    {
        new GuildUserBuilder(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(GuildBuilder.Build()).SetRoles(Enumerable.Empty<ulong>()).Build(),
        new GuildUserBuilder(Consts.UserId + 2, Consts.Username, Consts.Discriminator).SetGuild(GuildBuilder.Build()).SetRoles(new[] { Role.Id }).Build(),
    };

    private static readonly IGuild GuildData = GuildBuilder.SetGetUsersAction(new[] { GuildUser, AnotherUsers[0] }).Build();

    protected override IGuild Guild => GuildData;
    protected override IGuildUser User => GuildUser;

    protected override PointsChart CreateAction()
    {
        return InitAction(new PointsChart(DatabaseBuilder, TestServices.InitializedProvider.Value));
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));

        foreach (var user in AnotherUsers)
        {
            await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
            await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, user));
        }

        var random = new Random();
        for (var i = 0; i < 50; i++)
        {
            await Task.Delay(5);

            await Repository.AddAsync(CreateTransaction(random.Next(0, i), i % 2 == 0, Consts.UserId));
            await Repository.AddAsync(CreateTransaction(random.Next(0, i), i % 2 == 0, Consts.UserId + 1));
        }

        await Repository.CommitAsync();
    }

    private static Database.Entity.PointsTransaction CreateTransaction(int points, bool isReaction, ulong userId)
    {
        return new Database.Entity.PointsTransaction
        {
            GuildId = Consts.GuildId.ToString(),
            UserId = userId.ToString(),
            Points = points,
            AssingnedAt = DateTime.Now,
            MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            ReactionId = isReaction ? SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString() : ""
        };
    }

    [TestMethod]
    public async Task ProcessAsync_UserGraph_WithUsers()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync(ChartType.UserChart, AnotherUsers, ChartsFilter.All);
        CheckResult(result);
    }

    [TestMethod]
    public async Task ProcessAsync_UserGraph_WithoutUsers()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync(ChartType.UserChart, null, ChartsFilter.All);
        CheckResult(result);
    }

    [TestMethod]
    public async Task ProcessAsync_GuildGraph()
    {
        await InitDataAsync();

        var result = await Action.ProcessAsync(ChartType.GuildChart, null, ChartsFilter.All);
        CheckResult(result);
    }

    private static void CheckResult(TemporaryFile result)
    {
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Path);
        Assert.IsTrue(File.Exists(result.Path));
        File.Delete(result.Path);
    }
}
