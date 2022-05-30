using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using System;
using System.IO;

namespace GrillBot.Tests.App.Services.AuditLog;

[TestClass]
public class AuditLogClearingJobTests : JobTest<AuditLogClearingJob>
{
    protected override AuditLogClearingJob CreateJob()
    {
        var selfUser = new SelfUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var client = new ClientBuilder().SetSelfUser(selfUser).Build();

        var configuration = ConfigurationHelper.CreateConfiguration();
        var fileStorage = FileStorageHelper.Create(configuration);
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, DbFactory, interactionService);
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var counterManager = new CounterManager();
        var messageCache = new MessageCacheManager(discordClient, initManager, CacheBuilder, counterManager);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initManager);

        return new AuditLogClearingJob(loggingService, auditLogService, client, DbFactory, fileStorage, initManager);
    }

    [TestMethod]
    public async Task Execute_NoData()
    {
        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task Execute_WithData_NoFiles()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.MinValue,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 12345,
            DiscordAuditLogItemId = "12345"
        });

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345", Nickname = "Test", UsedInviteCode = "ABCD" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.AddAsync(new Invite() { Code = "ABCD", GuildId = "12345" });
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }

    [TestMethod]
    public async Task Execute_WithData_WithFiles()
    {
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.MinValue,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 12345,
            DiscordAuditLogItemId = "12345",
        };

        File.WriteAllText("Temp.txt", "asdf");
        item.Files.Add(new AuditLogFileMeta()
        {
            Filename = "Temp.txt",
            Size = 4
        });
        item.Files.Add(new AuditLogFileMeta() { Filename = "Temporary.txt" });

        await DbContext.AddAsync(item);
        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345", Nickname = "Test", UsedInviteCode = "ABCD" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.AddAsync(new Invite() { Code = "ABCD", GuildId = "12345" });
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }

    [TestMethod]
    public async Task Execute_WithData_Error()
    {
        await File.WriteAllBytesAsync("File.zip", new byte[] { 0, 1, 6, 8, 6 });
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.MinValue,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 12345,
            DiscordAuditLogItemId = "12345",
        };
        item.Files.Add(new AuditLogFileMeta() { Filename = "ddddd.txt" });

        await DbContext.AddAsync(item);
        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345", Nickname = "Test" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }
}
