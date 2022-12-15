using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.App.Services.User.Points;
using GrillBot.Cache.Services.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class ServicePointsIncrementTests : ApiActionTest<ServiceIncrementPoints>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override ServiceIncrementPoints CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User }).Build();

        var texts = new TextsBuilder().Build();
        var pointsService = new PointsService(DatabaseBuilder, TestServices.Configuration.Value, TestServices.Randomization.Value, texts);
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();

        return new ServiceIncrementPoints(ApiRequestContext, pointsService, client, texts);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_UserNotFound()
        => await Action.ProcessAsync(Consts.GuildId + 1, Consts.UserId, 100);

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        const int amount = 100;

        await Action.ProcessAsync(Consts.GuildId, Consts.UserId, amount);
        Repository.ClearChangeTracker();

        var points = await Repository.Points.ComputePointsOfUserAsync(Consts.GuildId, Consts.UserId);
        Assert.AreEqual(amount, points);
    }
}
