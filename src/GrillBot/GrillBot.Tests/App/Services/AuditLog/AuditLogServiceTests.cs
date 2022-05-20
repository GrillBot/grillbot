using Discord;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Moq;
using System;

namespace GrillBot.Tests.App.Services.AuditLog;

[TestClass]
public class AuditLogServiceTests : ServiceTest<AuditLogService>
{
    protected override AuditLogService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, DbFactory, CacheBuilder);
        var configuration = ConfigurationHelper.CreateConfiguration();
        var storage = FileStorageHelper.Create(configuration);

        return new AuditLogService(discordClient, DbFactory, messageCache, storage, initializationService);
    }

    [TestMethod]
    public async Task StoreItemsAsync_Success()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var guildUser = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var items = new List<AuditLogDataWrapper>()
        {
            new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", guild, processedUser: user, discordAuditLogItemId: "12345"),
            new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", files: new List<AuditLogFileMeta>() { new() { Filename = "File", Size = 1 } }),
            new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", guild, discordAuditLogItemId: "12345"),
            new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", guild, discordAuditLogItemId: "12345", createdAt: DateTime.Now),
            new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", guild, processedUser: guildUser, discordAuditLogItemId: "12345", createdAt: DateTime.MinValue),
            new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", guild, processedUser: guildUser, discordAuditLogItemId: "12345")
        };

        await Service.StoreItemsAsync(items);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task StoreItemAsync_Success()
    {
        var user = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await Service.StoreItemAsync(new AuditLogDataWrapper(AuditLogItemType.Warning, "{}", guild, processedUser: user, discordAuditLogItemId: "12345"));
        Assert.IsTrue(true);
    }

    private async Task FillDataAsync()
    {
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = AuditLogItemType.Command,
            Id = 12345,
            DiscordAuditLogItemId = "12345",
        };

        await DbContext.AddAsync(item);
        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345", Nickname = "Test" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();
    }

    [TestMethod]
    public async Task GetDiscordAuditLogIdsAsync_WithFilters()
    {
        await FillDataAsync();

        var channel = new ChannelBuilder().SetIdentity(Consts.ChannelId, Consts.ChannelName).Build();
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();
        var types = new[] { AuditLogItemType.InteractionCommand };

        var result = await Service.GetDiscordAuditLogIdsAsync(guild, channel, types, DateTime.MinValue);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetDiscordAuditLogIdsAsync_WithoutFilters()
    {
        await FillDataAsync();

        var result = await Service.GetDiscordAuditLogIdsAsync(null, null, null, DateTime.MinValue);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task GetGuildFromChannelAsync_DMs()
    {
        var channel = new Mock<IDMChannel>();

        var guild = await Service.GetGuildFromChannelAsync(channel.Object, 0);
        Assert.IsNull(guild);
    }

    [TestMethod]
    public async Task GetGuildFromChannelAsync_GuildChannel()
    {
        var channel = new TextChannelBuilder()
            .SetIdentity(Consts.ChannelId, Consts.ChannelName)
            .SetGuild(new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build()).Build();

        var guild = await Service.GetGuildFromChannelAsync(channel, 0);
        Assert.IsNotNull(guild);
    }

    [TestMethod]
    public async Task GetGuildFromChannelAsync_Unknown()
    {
        var guild = await Service.GetGuildFromChannelAsync(null, default);
        Assert.IsNull(guild);
    }

    [TestMethod]
    public async Task GetGuildFromChannelAsync_FromDB_Unknown()
    {
        var guild = await Service.GetGuildFromChannelAsync(null, 42);
        Assert.IsNull(guild);
    }
}
