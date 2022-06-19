using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuditLogControllerTests : ControllerTest<AuditLogController>
{
    protected override bool CanInitProvider() => false;

    protected override AuditLogController CreateController()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var fileStorage = new FileStorageMock(configuration);
        var mapper = AutoMapperHelper.CreateMapper();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new AuditLogApiService(DatabaseBuilder, mapper, fileStorage, AdminApiRequestContext, auditLogWriter);

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
        await Repository.AddAsync(new AuditLogItem
        {
            Id = 12345,
            CreatedAt = DateTime.UtcNow,
            Type = AuditLogItemType.Command,
            Data = ""
        });
        await Repository.CommitAsync();

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
        var item = new AuditLogItem
        {
            GuildChannel = new GuildChannel { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        item.Files.Add(new AuditLogFileMeta
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await Repository.AddAsync(item);
        await Repository.CommitAsync();

        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_WithFile_Exists()
    {
        var item = new AuditLogItem
        {
            GuildChannel = new GuildChannel { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        await File.WriteAllBytesAsync("Temp.txt", new byte[] { 1, 2, 3, 4, 5, 6 });
        item.Files.Add(new AuditLogFileMeta
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await Repository.AddAsync(item);
        await Repository.CommitAsync();

        var result = await AdminController.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
        Assert.IsFalse(File.Exists("Temp.txt"));
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithoutFilter_WithoutData()
    {
        var result = await AdminController.GetAuditLogListAsync(new AuditLogListParams());
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithFilter_WithData()
    {
        var filter = new AuditLogListParams
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            IgnoreBots = true,
            ProcessedUserIds = new List<string> { Consts.UserId.ToString() },
            Types = Enum.GetValues<AuditLogItemType>().ToList()
        };

        await Repository.AddAsync(new AuditLogItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.Command
        });
        await Repository.AddCollectionAsync(Enum.GetValues<AuditLogItemType>().Where(o => o > 0).Select(o => new AuditLogItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = o
        }));

        await Repository.AddAsync(new Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.AddAsync(new GuildChannel { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await Repository.AddAsync(new GuildUser { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await Repository.AddAsync(new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await AdminController.GetAuditLogListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithExcludes_WithData()
    {
        var filter = new AuditLogListParams
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            IgnoreBots = true,
            ProcessedUserIds = new List<string> { Consts.UserId.ToString() },
            Types = new List<AuditLogItemType> { AuditLogItemType.Info },
            ExcludedTypes = new List<AuditLogItemType> { AuditLogItemType.MemberRoleUpdated }
        };

        await Repository.AddAsync(new AuditLogItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.Command
        });
        await Repository.AddCollectionAsync(Enum.GetValues<AuditLogItemType>().Where(o => o > 0).Select(o => new AuditLogItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = o
        }));

        await Repository.AddAsync(new Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.AddAsync(new GuildChannel { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await Repository.AddAsync(new GuildUser { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await Repository.AddAsync(new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await AdminController.GetAuditLogListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithExtendedFilters_WithData()
    {
        var filter = new AuditLogListParams
        {
            Types = Enum.GetValues<AuditLogItemType>().ToList(),
            CommandFilter = new ExecutionFilter
            {
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue },
                Name = "dd",
                WasSuccess = true,
            },
            ErrorFilter = new TextFilter { Text = "T" },
            InfoFilter = new TextFilter { Text = "T" },
            InteractionFilter = new ExecutionFilter
            {
                WasSuccess = true,
                Name = "ddddddd",
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue }
            },
            JobFilter = new ExecutionFilter
            {
                WasSuccess = true,
                Name = "ddddddd",
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue }
            },
            WarningFilter = new TextFilter { Text = "T" },
            Ids = string.Join(", ", Enumerable.Range(0, 1000).Select(o => o.ToString()))
        };

        static AuditLogItem CreateItem(object data, AuditLogItemType type)
        {
            return new AuditLogItem
            {
                ChannelId = Consts.ChannelId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Data = JsonConvert.SerializeObject(data, AuditLogWriter.SerializerSettings),
                GuildId = Consts.GuildId.ToString(),
                ProcessedUserId = Consts.UserId.ToString(),
                Type = type
            };
        }

        await Repository.AddAsync(CreateItem(new CommandExecution { Command = "A", Duration = 50, IsSuccess = true }, AuditLogItemType.Command));
        await Repository.AddAsync(new Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.AddAsync(new GuildChannel { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await Repository.AddAsync(new GuildUser { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await Repository.AddAsync(new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await AdminController.GetAuditLogListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_ItemNotFound()
    {
        var result = await AdminController.GetFileContentAsync(1, 1);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContent_FileItemNotFound()
    {
        await Repository.AddAsync(new AuditLogItem
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = Consts.GuildId.ToString(),
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.Command,
            Id = 12345
        });

        await Repository.AddAsync(new Guild { Id = Consts.GuildId.ToString(), Name = Consts.GuildName });
        await Repository.AddAsync(new GuildChannel { Name = Consts.ChannelName, GuildId = Consts.GuildId.ToString(), ChannelId = Consts.ChannelId.ToString() });
        await Repository.AddAsync(new GuildUser { GuildId = Consts.GuildId.ToString(), UserId = Consts.UserId.ToString() });
        await Repository.AddAsync(new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_FileNotExists()
    {
        var item = new AuditLogItem
        {
            GuildChannel = new GuildChannel { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        item.Files.Add(new AuditLogFileMeta
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await Repository.AddAsync(item);
        await Repository.CommitAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetFileContentAsync_Success()
    {
        var item = new AuditLogItem
        {
            GuildChannel = new GuildChannel { ChannelId = Consts.ChannelId.ToString(), GuildId = Consts.GuildId.ToString(), Name = Consts.ChannelName },
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            Guild = new Guild { Name = Consts.GuildName, Id = Consts.GuildId.ToString() },
            ProcessedGuildUser = new GuildUser
            {
                GuildId = Consts.GuildId.ToString(),
                User = new User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator }
            },
            ProcessedUserId = Consts.UserId.ToString(),
            Type = AuditLogItemType.MessageDeleted,
            Id = 12345,
        };

        await File.WriteAllBytesAsync("Temp.txt", new byte[] { 1, 2, 3, 4, 5, 6 });
        item.Files.Add(new AuditLogFileMeta
        {
            Filename = "Temp.txt",
            Id = 123,
            Size = 123456
        });

        await Repository.AddAsync(item);
        await Repository.CommitAsync();

        var result = await AdminController.GetFileContentAsync(12345, 123);
        CheckResult<FileContentResult>(result);
    }

    [TestMethod]
    public void AuditLogListParams_ModelStateValidation()
    {
        var @params = new AuditLogListParams
        {
            Ids = "0,1,3;",
            Types = new List<AuditLogItemType> { AuditLogItemType.Info },
            ExcludedTypes = new List<AuditLogItemType> { AuditLogItemType.Info }
        };
        var validationContext = new ValidationContext(@params);

        var result = @params.Validate(validationContext).ToList();
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task HandleClientAppMessageAsync()
    {
        var requests = new[]
        {
            new ClientLogItemRequest { IsInfo = true, Content = "Content" },
            new ClientLogItemRequest { IsWarning = true, Content = "Content" },
            new ClientLogItemRequest { IsError = true, Content = "Content" }
        };

        foreach (var request in requests)
        {
            var result = await AdminController.HandleClientAppMessageAsync(request);
            CheckResult<OkResult>(result);
        }
    }
}
