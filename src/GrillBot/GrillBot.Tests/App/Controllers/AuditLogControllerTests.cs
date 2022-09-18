using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuditLogControllerTests : ControllerTest<AuditLogController>
{
    protected override AuditLogController CreateController()
    {
        var fileStorage = new FileStorageMock(TestServices.Configuration.Value);
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new AuditLogApiService(DatabaseBuilder, fileStorage, ApiRequestContext, auditLogWriter);

        return new AuditLogController(apiService, ServiceProvider);
    }

    protected override void Cleanup()
    {
        if (File.Exists("Temp.txt"))
            File.Delete("Temp.txt");
    }

    [TestMethod]
    public async Task GetFileContentAsync_ItemNotFound()
    {
        var result = await Controller.GetFileContentAsync(1, 1);
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

        var result = await Controller.GetFileContentAsync(12345, 123);
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

        var result = await Controller.GetFileContentAsync(12345, 123);
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

        var result = await Controller.GetFileContentAsync(12345, 123);
        CheckResult<FileContentResult>(result);
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
            var result = await Controller.HandleClientAppMessageAsync(request);
            CheckResult<OkResult>(result);
        }
    }
}
