using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Database.Entity;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.AuditLog;

[TestClass]
public class AuditLogClearingJobTests : JobTest<AuditLogClearingJob>
{
    protected override AuditLogClearingJob CreateJob()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var dbFactory = new DbContextBuilder();
        var fileStorage = FileStorageHelper.Create(configuration);
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var loggerFactory = LoggingHelper.CreateLoggerFactory();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var loggingService = new LoggingService(discordClient, commandsService, loggerFactory, configuration, dbFactory, interactionService);
        DbContext = dbFactory.Create();

        return new AuditLogClearingJob(configuration, dbFactory, fileStorage, loggingService);
    }

    public override void Cleanup()
    {
        DbContext.ChangeTracker.Clear();
        DbContext.AuditLogs.RemoveRange(DbContext.AuditLogs.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.Channels.RemoveRange(DbContext.Channels.AsEnumerable());
        DbContext.GuildUsers.RemoveRange(DbContext.GuildUsers.AsEnumerable());
        DbContext.AuditLogFiles.RemoveRange(DbContext.AuditLogFiles.AsEnumerable());
        DbContext.Invites.RemoveRange(DbContext.Invites.AsEnumerable());
        DbContext.SaveChanges();
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
            CreatedAt = DateTime.UtcNow,
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
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.AddAsync(new Invite() { Code = "ABCD" });
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
            CreatedAt = DateTime.UtcNow,
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
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.AddAsync(new Invite() { Code = "ABCD" });
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public async Task Execute_WithData_Error()
    {
        await File.WriteAllBytesAsync("File.zip", new byte[] { 0, 1, 6, 8, 6 });
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
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
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }
}
