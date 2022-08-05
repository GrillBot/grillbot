using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Discord;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.User.Points;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User.Points;

[TestClass]
public class PointsServiceTests : ServiceTest<PointsService>
{
    private IGuild Guild { get; set; }
    private IGuildUser GuildUser { get; set; }
    private IUser User { get; set; }

    protected override PointsService CreateService()
    {
        User = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator)
            .SetAvatar(Consts.AvatarId).Build();
        var userBuilder = new GuildUserBuilder().SetIdentity(User).SetAvatar(Consts.AvatarId);
        Guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetUserAction(userBuilder.Build())
            .Build();
        GuildUser = userBuilder.SetGuild(Guild).Build();

        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = new CounterManager();
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        var randomization = new RandomizationService();
        var profilePicture = new ProfilePictureManager(CacheBuilder, counterManager);

        return new PointsService(discordClient, DatabaseBuilder, configuration, messageCache, randomization,
            profilePicture);
    }

    public override void Cleanup()
    {
        base.Cleanup();

        if (File.Exists("File.zip"))
            File.Delete("File.zip");
    }

    #region ServiceActions

    [TestMethod]
    public async Task IncrementPointsAsync()
    {
        await Service.IncrementPointsAsync(GuildUser, 100);
        Repository.ClearChangeTracker();

        var userPoints = await Repository.Points.ComputePointsOfUserAsync(Guild.Id, GuildUser.Id);
        Assert.AreEqual(100, userPoints);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_SameUsers()
    {
        await Service.TransferPointsAsync(GuildUser, GuildUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_FromBot()
    {
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).Build();

        await Service.TransferPointsAsync(GuildUser, toUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_ToBot()
    {
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).AsBot().Build();

        await Service.TransferPointsAsync(GuildUser, toUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_SourceNotInDb()
    {
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).Build();

        await Service.TransferPointsAsync(GuildUser, toUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_InsufficientAmount()
    {
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).Build();

        await Repository.Guild.GetOrCreateRepositoryAsync(Guild);
        await Repository.User.GetOrCreateUserAsync(GuildUser);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(GuildUser);
        await Repository.CommitAsync();

        await Service.TransferPointsAsync(GuildUser, toUser, 50);
    }

    [TestMethod]
    public async Task TransferPointsAsync_Success()
    {
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator)
            .SetGuild(Guild).Build();

        await Service.IncrementPointsAsync(GuildUser, 100);
        await Service.TransferPointsAsync(GuildUser, toUser, 50);
        Repository.ClearChangeTracker();

        var fromUserPoints = await Repository.Points.ComputePointsOfUserAsync(Guild.Id, GuildUser.Id);
        var toUserPoints = await Repository.Points.ComputePointsOfUserAsync(Guild.Id, toUser.Id);

        Assert.AreEqual(50, fromUserPoints);
        Assert.AreEqual(50, toUserPoints);
    }

    #endregion
}
