using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Discord;
using System.IO;
using GrillBot.Common.Managers.Logging;

namespace GrillBot.Tests.App.Services.AuditLog;

[TestClass]
public class AuditLogClearingJobTests : JobTest<AuditLogClearingJob>
{
    protected override AuditLogClearingJob CreateJob()
    {
        var selfUser = new SelfUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        var client = new ClientBuilder().SetSelfUser(selfUser).Build();

        var configuration = TestServices.Configuration.Value;
        var fileStorage = new FileStorageMock(configuration);
        var discordClient = DiscordHelper.CreateClient();
        var commandsService = DiscordHelper.CreateCommandsService();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var auditClearingHelper = new AuditClearingHelper(fileStorage);
        var loggingManager = new LoggingManager(discordClient, commandsService, interactionService, TestServices.EmptyProvider.Value);

        return new AuditLogClearingJob(auditLogWriter, client, DatabaseBuilder, initManager, auditClearingHelper, loggingManager);
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
        await Repository.AddAsync(new AuditLogItem
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

        await Repository.AddAsync(new Database.Entity.Guild { Id = "12345", Name = "Guild" });
        await Repository.AddAsync(new GuildChannel { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await Repository.AddAsync(new GuildUser { GuildId = "12345", UserId = "12345", Nickname = "Test", UsedInviteCode = "ABCD" });
        await Repository.AddAsync(new Database.Entity.User { Id = "12345", Username = "Username", Discriminator = "1234" });
        await Repository.AddAsync(new Invite { Code = "ABCD", GuildId = "12345" });
        await Repository.CommitAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }

    [TestMethod]
    public async Task Execute_WithData_WithFiles()
    {
        var item = new AuditLogItem
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

        await File.WriteAllTextAsync("Temp.txt", "asdf");
        item.Files.Add(new AuditLogFileMeta
        {
            Filename = "Temp.txt",
            Size = 4
        });
        item.Files.Add(new AuditLogFileMeta { Filename = "Temporary.txt" });

        await Repository.AddAsync(item);
        await Repository.AddAsync(new Database.Entity.Guild { Id = "12345", Name = "Guild" });
        await Repository.AddAsync(new GuildChannel { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await Repository.AddAsync(new GuildUser { GuildId = "12345", UserId = "12345", Nickname = "Test", UsedInviteCode = "ABCD" });
        await Repository.AddAsync(new Database.Entity.User { Id = "12345", Username = "Username", Discriminator = "1234" });
        await Repository.AddAsync(new Invite { Code = "ABCD", GuildId = "12345" });
        await Repository.CommitAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }

    [TestMethod]
    public async Task Execute_WithData_Error()
    {
        await File.WriteAllBytesAsync("File.zip", new byte[] { 0, 1, 6, 8, 6 });
        var item = new AuditLogItem
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
        item.Files.Add(new AuditLogFileMeta { Filename = "ddddd.txt" });

        await Repository.AddAsync(item);
        await Repository.AddAsync(new Database.Entity.Guild { Id = "12345", Name = "Guild" });
        await Repository.AddAsync(new GuildChannel { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await Repository.AddAsync(new GuildUser { GuildId = "12345", UserId = "12345", Nickname = "Test" });
        await Repository.AddAsync(new Database.Entity.User { Id = "12345", Username = "Username", Discriminator = "1234" });
        await Repository.CommitAsync();

        var context = CreateContext();
        await Job.Execute(context);
        Assert.IsTrue(true);
        if (File.Exists("File.zip")) File.Delete("File.zip");
    }
}
