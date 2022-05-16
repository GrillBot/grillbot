using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            GuildChannel = new GuildChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild() { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser()
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
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
        await DbContext.SaveChangesAsync();

        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_WithFile_Exists()
    {
        var item = new AuditLogItem()
        {
            GuildChannel = new GuildChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild() { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser()
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
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
            ChannelId = Consts.ChannelId.ToString(),
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            IgnoreBots = true,
            ProcessedUserIds = new List<string>() { Consts.UserId.ToString() },
            Types = Enum.GetValues<AuditLogItemType>().ToList()
        };

        await DbContext.AddAsync(new AuditLogItem()
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = null,
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.Command
        });
        await DbContext.AddRangeAsync(Enum.GetValues<AuditLogItemType>().Where(o => o > 0).Select(o => new AuditLogItem()
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = o
        }));

        await DbContext.AddAsync(new Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetAuditLogListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithExtendedFilters_WithData()
    {
        var filter = new AuditLogListParams()
        {
            Types = Enum.GetValues<AuditLogItemType>().ToList(),
            CommandFilter = new()
            {
                Duration = new() { From = int.MinValue, To = int.MaxValue },
                Name = "dd",
                WasSuccess = true,
            },
            ErrorFilter = new() { Text = "T" },
            InfoFilter = new() { Text = "T" },
            InteractionFilter = new()
            {
                WasSuccess = true,
                Name = "ddddddd",
                Duration = new() { From = int.MinValue, To = int.MaxValue }
            },
            JobFilter = new()
            {
                WasSuccess = true,
                Name = "ddddddd",
                Duration = new() { From = int.MinValue, To = int.MaxValue }
            },
            WarningFilter = new() { Text = "T" }
        };

        static AuditLogItem createItem(object data, AuditLogItemType type)
        {
            return new AuditLogItem()
            {
                ChannelId = Consts.ChannelId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Data = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings),
                GuildId = Consts.GuildId.ToString(),
                ProcessedUserId = Consts.UserId.ToString(),
                Type = type
            };
        }

        await DbContext.AddAsync(createItem(new CommandExecution() { Command = "A", Duration = 50, IsSuccess = true }, AuditLogItemType.Command));
        await DbContext.AddAsync(new Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
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
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.Command,
            Id = 12345
        });

        await DbContext.AddAsync(new Guild() { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await DbContext.AddAsync(new GuildChannel() { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await DbContext.AddAsync(new GuildUser() { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await DbContext.AddAsync(new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_FileNotExists()
    {
        var item = new AuditLogItem()
        {
            GuildChannel = new GuildChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild() { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser()
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
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
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_Success()
    {
        var item = new AuditLogItem()
        {
            GuildChannel = new GuildChannel() { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild() { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser()
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
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
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123, CancellationToken.None);
        CheckResult<FileContentResult>(result);
    }
}
