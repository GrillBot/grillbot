using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuditLogControllerTests : ControllerTest<AuditLogController>
{
    protected override AuditLogController CreateController()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var fileStorage = FileStorageHelper.Create(configuration);
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new AuditLogApiService(DbFactory, mapper, fileStorage);

        return new AuditLogController(apiService);
    }

    public override void Cleanup()
    {
        if (File.Exists("Temp.txt"))
            File.Delete("Temp.txt");
    }

    [TestMethod]
    public async Task RemoveItemAsync_Found()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Id = 12345,
            CreatedAt = DateTime.UtcNow,
            Type = AuditLogItemType.Command
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_NotFound()
    {
        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_WithFile_NotExists()
    {
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        item.Files.Add(new AuditLogFileMeta()
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await DbContext.AddAsync(item);

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_WithFile_Exists()
    {
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        await File.WriteAllBytesAsync("Temp.txt", new byte[] { 1, 2, 3, 4, 5, 6 });
        item.Files.Add(new AuditLogFileMeta()
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await DbContext.AddAsync(item);

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
        Assert.IsFalse(File.Exists("Temp.txt"));
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithoutFilter_WithoutData()
    {
        var result = await AdminController.GetAuditLogListAsync(new AuditLogListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithFilter_WithData()
    {
        var filter = new AuditLogListParams()
        {
            ChannelId = "12345",
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            GuildId = "12345",
            IgnoreBots = true,
            ProcessedUserIds = new List<string>() { "12345" },
            Types = Enum.GetValues<AuditLogItemType>().ToList()
        };

        await DbContext.AddRangeAsync(new[]
        {
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = null, GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.Command },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.Command },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "--", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.Info },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.ChannelDeleted },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.ChannelUpdated },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.EmojiDeleted },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.GuildUpdated },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.MemberUpdated },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.MessageDeleted },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.MessageEdited },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.OverwriteCreated },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.OverwriteUpdated },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.Unban },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.UserJoined },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.UserLeft },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.InteractionCommand },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.ThreadDeleted },
            new AuditLogItem() { ChannelId = "12345", CreatedAt = DateTime.UtcNow, Data = "{}", GuildId = "12345", ProcessedUserId = "12345", Type = AuditLogItemType.JobCompleted }
        });

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetAuditLogListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_ItemNotFound()
    {
        var result = await AdminController.GetFileContentAsync(1, 1, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContent_FileItemNotFound()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = AuditLogItemType.Command,
            Id = 12345
        });

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_FileNotExists()
    {
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        item.Files.Add(new AuditLogFileMeta()
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await DbContext.AddAsync(item);

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_Success()
    {
        var item = new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        await File.WriteAllBytesAsync("Temp.txt", new byte[] { 1, 2, 3, 4, 5, 6 });
        item.Files.Add(new AuditLogFileMeta()
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await DbContext.AddAsync(item);

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123, CancellationToken.None);
        CheckResult<FileContentResult>(result);
    }
}
