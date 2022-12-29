using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Discord;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.App.Services.User.Points;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Points;

[TestClass]
public class ServiceTransferPointsTests : ApiActionTest<ServiceTransferPoints>
{
    private IGuild Guild { get; set; }
    private IGuildUser User { get; set; }

    protected override ServiceTransferPoints CreateAction()
    {
        var guildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName);
        User = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        var anotherUser = new GuildUserBuilder(User).SetId(Consts.UserId + 1).SetGuild(guildBuilder.Build()).Build();
        var botUser = new GuildUserBuilder(User).SetId(Consts.UserId + 2).AsBot().Build();
        Guild = guildBuilder.SetGetUsersAction(new[] { User, botUser, anotherUser }).Build();

        var texts = new TextsBuilder()
            .AddText("Points/Service/Transfer/UserIsBot", "cs", "UserIsBot{0}")
            .AddText("Points/Service/Transfer/InsufficientAmount", "cs", "InsufficientAmount{0}")
            .Build();
        var pointsService = new PointsService(DatabaseBuilder, TestServices.Configuration.Value, TestServices.Random.Value, texts);
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();

        return new ServiceTransferPoints(ApiRequestContext, pointsService, client, texts);
    }

    private async Task InitSummariesAsync()
    {
        await Repository.Guild.GetOrCreateGuildAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(User);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(User);

        await Repository.AddAsync(new PointsTransaction
        {
            Points = 10,
            UserId = Consts.UserId.ToString(),
            GuildId = Consts.GuildId.ToString(),
            AssingnedAt = DateTime.Now,
            MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
            ReactionId = ""
        });

        await Repository.CommitAsync();
        Repository.ClearChangeTracker();
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_SameUsers()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId, Consts.UserId, 1);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_UserNotFound()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId, Consts.UserId + 3, 1);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_SourceUserIsBot()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId + 2, Consts.UserId, 1);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_DestUserIsBot()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId, Consts.UserId + 2, 1);

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_GuildNotFound()
        => await Action.ProcessAsync(Consts.GuildId + 1, Consts.UserId, Consts.UserId + 1, 1);

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_InsufficientAmount()
        => await Action.ProcessAsync(Consts.GuildId, Consts.UserId, Consts.UserId + 1, 1);

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        const int amount = 5;
        await InitSummariesAsync();
        await Action.ProcessAsync(Consts.GuildId, Consts.UserId, Consts.UserId + 1, amount);
        Repository.ClearChangeTracker();

        var toUserPoints = await Repository.Points.ComputePointsOfUserAsync(Consts.GuildId, Consts.UserId + 1);
        Assert.AreEqual(amount, toUserPoints);
    }
}
