using Discord;
using Discord.WebSocket;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.FileStorage;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GrillBot.Tests.App.Services.AuditLog;

[TestClass]
public class AuditLogServiceTests
{
    private static ServiceProvider CreateService(out AuditLogService auditLogService)
    {
        var container = DIHelpers.CreateContainer(services =>
        {
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<GrillBot.App.Services.MessageCache.MessageCache>();
            services.AddSingleton<FileStorageFactory>();
            services.AddSingleton(ConfigHelpers.CreateConfiguration());
            services.AddSingleton<DiscordInitializationService>();
            services.AddSingleton<AuditLogService>();
            services.AddLogging();
        }, true);

        auditLogService = container.GetService<AuditLogService>();
        return container;
    }

    #region GetGuildFromChannel

    [TestMethod]
    public void GetGuildFromChannel_DMs()
    {
        var mock = new Mock<IDMChannel>();

        using var _ = CreateService(out var service);
        var result = service.GetGuildFromChannelAsync(mock.Object, 0).Result;

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetGuildFromChannel_GuildChannel()
    {
        var guild = new Mock<IGuild>();
        guild.Setup(o => o.Id).Returns(12345);
        var channel = new Mock<IGuildChannel>();
        channel.Setup(o => o.Guild).Returns(guild.Object);

        using var _ = CreateService(out var service);
        var result = service.GetGuildFromChannelAsync(channel.Object, 0).Result;

        Assert.IsNotNull(result);
        Assert.AreEqual((ulong)12345, result.Id);
    }

    [TestMethod]
    public void GetGuildFromChannel_Empty()
    {
        using var _ = CreateService(out var service);
        var result = service.GetGuildFromChannelAsync(null, default).Result;

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetGuildFromChannel_FromDB_NotFound()
    {
        using var container = CreateService(out var service);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
        dbContext.Channels.RemoveRange(dbContext.Channels.AsEnumerable());
        dbContext.Guilds.RemoveRange(dbContext.Guilds.AsEnumerable());
        dbContext.SaveChanges();

        var result = service.GetGuildFromChannelAsync(null, 1).Result;
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetGuildFromChannel_FromDB_Found()
    {
        using var container = CreateService(out var service);
        var factory = container.GetService<GrillBotContextFactory>();
        var dbContext = factory.Create();
        dbContext.Channels.RemoveRange(dbContext.Channels.AsEnumerable());
        dbContext.Guilds.RemoveRange(dbContext.Guilds.AsEnumerable());
        dbContext.Channels.Add(new GuildChannel() { ChannelId = "1", Name = "Channel", GuildId = "2" });
        dbContext.Guilds.Add(new Guild() { Id = "2", Name = "Guild" });
        dbContext.SaveChanges();

        var channel = new Mock<IChannel>();
        var result = service.GetGuildFromChannelAsync(channel.Object, 1).Result;
        Assert.IsNull(result);
    }

    #endregion

    #region StoreItem

    [TestMethod]
    public void StoreItem_NullAuditLogId()
    {
        using var _ = CreateService(out var service);

        var guild = new Mock<IGuild>();
        guild.Setup(o => o.Roles).Returns(new List<IRole>().AsReadOnly());
        guild.Setup(o => o.Name).Returns("Name");
        guild.Setup(o => o.Id).Returns(1);
        var user = new Mock<IUser>();
        user.Setup(o => o.Id).Returns(1);
        user.Setup(o => o.Username).Returns("User");

        service.StoreItemAsync(AuditLogItemType.ChannelCreated, guild.Object, null, user.Object, "{}", null, null, null).Wait();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void StoreItem_AuditLogIdAsString()
    {
        using var _ = CreateService(out var service);

        var guild = new Mock<IGuild>();
        guild.Setup(o => o.Roles).Returns(new List<IRole>().AsReadOnly());
        guild.Setup(o => o.Name).Returns("Name");
        guild.Setup(o => o.Id).Returns(1);
        var user = new Mock<IGuildUser>();
        user.Setup(o => o.Id).Returns(1);
        user.Setup(o => o.Username).Returns("User");
        user.Setup(o => o.Nickname).Returns("User");

        service.StoreItemAsync(AuditLogItemType.ChannelCreated, guild.Object, null, user.Object, "{}", "12345", CancellationToken.None, null).Wait();
        Assert.IsTrue(true);
    }

    [TestMethod]
    public void StoreItem_AuditLogIdAsUlong()
    {
        using var _ = CreateService(out var service);

        var guild = new Mock<IGuild>();
        guild.Setup(o => o.Roles).Returns(new List<IRole>().AsReadOnly());
        guild.Setup(o => o.Name).Returns("Name");
        guild.Setup(o => o.Id).Returns(1);

        service.StoreItemAsync(AuditLogItemType.ChannelCreated, guild.Object, null, null, "{}", (ulong)12345, CancellationToken.None, null).Wait();
        Assert.IsTrue(true);
    }

    [TestMethod]
    [ExpectedException(typeof(AggregateException))]
    public void StoreItem_AuditLogIdInvalid()
    {
        using var _ = CreateService(out var service);

        var guild = new Mock<IGuild>();
        guild.Setup(o => o.Roles).Returns(new List<IRole>().AsReadOnly());
        guild.Setup(o => o.Name).Returns("Name");
        guild.Setup(o => o.Id).Returns(1);

        service.StoreItemAsync(AuditLogItemType.ChannelCreated, guild.Object, null, null, "{}", new object(), CancellationToken.None, null).Wait();
    }

    [TestMethod]
    public void StoreItem_WithAttachments()
    {
        using var _ = CreateService(out var service);

        var guild = new Mock<IGuild>();
        guild.Setup(o => o.Roles).Returns(new List<IRole>().AsReadOnly());
        guild.Setup(o => o.Name).Returns("Name");
        guild.Setup(o => o.Id).Returns(1);

        var attachments = new List<AuditLogFileMeta>()
        {
            new AuditLogFileMeta() { Filename = "File" }
        };

        service.StoreItemAsync(AuditLogItemType.ChannelCreated, guild.Object, null, null, "{}", (ulong)12345, CancellationToken.None, attachments).Wait();
        Assert.IsTrue(true);
    }

    #endregion

    #region GetDiscordAuditLogIds

    [TestMethod]
    public void GetDiscordAuditLogIds_NoData_NoFilter()
    {
        using var container = CreateService(out var service);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));

        var ids = service.GetDiscordAuditLogIdsAsync(dbContext, null, null, null, DateTime.MaxValue).Result;
        Assert.AreEqual(0, ids.Count);
    }

    [TestMethod]
    public void GetDiscordAuditLogIds_NoData_WithFilter()
    {
        using var container = CreateService(out var service);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));

        var guild = new Mock<IGuild>();
        var channel = new Mock<IChannel>();
        var types = new[] { AuditLogItemType.ChannelCreated };

        var ids = service.GetDiscordAuditLogIdsAsync(dbContext, guild.Object, channel.Object, types, DateTime.MaxValue).Result;
        Assert.AreEqual(0, ids.Count);
    }

    [TestMethod]
    public void GetDiscordAuditLogIds_WithData()
    {
        using var container = CreateService(out var service);
        var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
        dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.AsEnumerable());
        dbContext.AuditLogs.Add(new AuditLogItem() { GuildId = "1", ChannelId = "1", Type = AuditLogItemType.ChannelCreated, DiscordAuditLogItemId = "12345", CreatedAt = DateTime.Now });
        dbContext.SaveChanges();

        var types = new[] { AuditLogItemType.ChannelCreated };
        var ids = service.GetDiscordAuditLogIdsAsync(dbContext, null, null, types, DateTime.MinValue).Result;
        Assert.AreEqual(1, ids.Count);
    }

    #endregion
}
