using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class ComputeUserPointsTests : ApiActionTest<ComputeUserPoints>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override ComputeUserPoints CreateAction()
    {
        var userBuilder = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator);
        Guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetUsersAction(new[] { userBuilder.Build() }).Build();
        User = userBuilder.SetGuild(Guild).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).SetGetUserAction(User).Build();

        return new ComputeUserPoints(ApiRequestContext, DatabaseBuilder, client, TestServices.AutoMapper.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_Private()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(User));
        await Repository.AddAsync(Database.Entity.GuildUser.FromDiscord(Guild, User));
        await Repository.AddAsync(new Database.Entity.PointsTransactionSummary
        {
            Day = DateTime.Today,
            GuildId = Consts.GuildId.ToString(),
            MessagePoints = 1,
            ReactionPoints = 1,
            UserId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(Consts.UserId);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(2, result.Sum(o => o.TotalPoints));
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Public()
    {
        var result = await Action.ProcessAsync(null);
        Assert.AreEqual(0, result.Count);
    }
}
