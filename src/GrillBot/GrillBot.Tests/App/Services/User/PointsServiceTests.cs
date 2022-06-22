using System;
using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Services;
using GrillBot.App.Services.User;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.User;

[TestClass]
public class PointsServiceTests : ServiceTest<PointsService>
{
    protected override PointsService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = new CounterManager();
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        var randomization = new RandomizationService();
        var profilePicture = new ProfilePictureManager(CacheBuilder, counterManager);

        return new PointsService(discordClient, DatabaseBuilder, configuration, messageCache, randomization, profilePicture);
    }

    [TestMethod]
    public async Task IncrementPointsAsync()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Service.IncrementPointsAsync(guildUser, 100);

        var userEntity = await Repository.GuildUser.FindGuildUserAsync(guildUser);
        Assert.IsNotNull(userEntity);
        Assert.AreEqual(100, userEntity.Points);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_SameUsers()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Service.TransferPointsAsync(guildUser, guildUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_FromBot()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var fromUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).AsBot().Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Service.TransferPointsAsync(fromUser, toUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_ToBot()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var fromUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).AsBot().Build();

        await Service.TransferPointsAsync(fromUser, toUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_SourceNotInDb()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var fromUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Service.TransferPointsAsync(fromUser, toUser, 50);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    [ExcludeFromCodeCoverage]
    public async Task TransferPointsAsync_InsufficientAmount()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var fromUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.Guild.GetOrCreateRepositoryAsync(guild);
        await Repository.User.GetOrCreateUserAsync(fromUser);
        await Repository.GuildUser.GetOrCreateGuildUserAsync(fromUser);
        await Repository.CommitAsync();

        await Service.TransferPointsAsync(fromUser, toUser, 50);
    }

    [TestMethod]
    public async Task TransferPointsAsync_Success()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var fromUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();
        var toUser = new GuildUserBuilder().SetIdentity(Consts.UserId + 1, Consts.Username, Consts.Discriminator).SetGuild(guild).Build();

        await Repository.Guild.GetOrCreateRepositoryAsync(guild);
        await Repository.User.GetOrCreateUserAsync(fromUser);
        var fromUserEntity = await Repository.GuildUser.GetOrCreateGuildUserAsync(fromUser);
        fromUserEntity.Points = 100;
        await Repository.CommitAsync();
        Repository.ClearChangeTracker();

        await Service.TransferPointsAsync(fromUser, toUser, 50);

        var toUserEntity = await Repository.GuildUser.FindGuildUserAsync(toUser);
        Assert.IsNotNull(toUserEntity);
        Assert.AreEqual(50, toUserEntity.Points);

        fromUserEntity = await Repository.GuildUser.FindGuildUserAsync(fromUser);
        Assert.IsNotNull(fromUserEntity);
        Assert.AreEqual(50, fromUserEntity.Points);
    }
}
