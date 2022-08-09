using Discord;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Discord;
using Moq;

namespace GrillBot.Tests.App.Services.AuditLog;

[TestClass]
public class AuditLogServiceTests : ServiceTest<AuditLogService>
{
    protected override AuditLogService CreateService()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, TestServices.CounterManager.Value);
        var storage = new FileStorageMock(TestServices.Configuration.Value);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);

        return new AuditLogService(discordClient, DatabaseBuilder, messageCache, storage, initManager, auditLogWriter);
    }

    private async Task FillDataAsync()
    {
        var item = new AuditLogItem
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

        await Repository.AddAsync(item);
        await Repository.AddAsync(new Database.Entity.Guild { Id = "12345", Name = "Guild" });
        await Repository.AddAsync(new GuildChannel { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await Repository.AddAsync(new GuildUser { GuildId = "12345", UserId = "12345", Nickname = "Test" });
        await Repository.AddAsync(new Database.Entity.User { Id = "12345", Username = "Username", Discriminator = "1234" });
        await Repository.CommitAsync();
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
