using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using GrillBot.Database.Entity;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuditLogControllerTests : ControllerTest<AuditLogController>
{
    protected override AuditLogController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var dbFactory = new DbContextBuilder();
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var cache = new MessageCache(discordClient, initializationService);
        var fileStorage = FileStorageHelper.Create();
        var auditLogService = new AuditLogService(discordClient, dbFactory, cache, fileStorage, initializationService);
        DbContext = dbFactory.Create();

        return new AuditLogController(auditLogService, DbContext, fileStorage);
    }

    public override void Cleanup()
    {
        DbContext.AuditLogs.RemoveRange(DbContext.AuditLogs.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.Guilds.RemoveRange(DbContext.Guilds.AsEnumerable());
        DbContext.Channels.RemoveRange(DbContext.Channels.AsEnumerable());
        DbContext.GuildUsers.RemoveRange(DbContext.GuildUsers.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task RemoveItemAsync_Found()
    {
        await DbContext.AddAsync(new AuditLogItem()
        {
            Id = 12345,
            CreatedAt = DateTime.UtcNow,
            Type = Database.Enums.AuditLogItemType.Command
        });
        await DbContext.SaveChangesAsync();

        var result = await Controller.RemoveItemAsync(12345);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_NotFound()
    {
        var result = await Controller.RemoveItemAsync(12345);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task GetAuditLogListAsync_WithoutFilter_WithoutData()
    {
        var result = await Controller.GetAuditLogListAsync(new AuditLogListParams());
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
            Types = new List<Database.Enums.AuditLogItemType>() { Database.Enums.AuditLogItemType.Command }
        };

        await DbContext.AddAsync(new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = Database.Enums.AuditLogItemType.Command
        });

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetAuditLogListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<AuditLogListItem>>(result);
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
        await DbContext.AddAsync(new AuditLogItem()
        {
            ChannelId = "12345",
            CreatedAt = DateTime.UtcNow,
            Data = "{}",
            GuildId = "12345",
            ProcessedUserId = "12345",
            Type = Database.Enums.AuditLogItemType.Command,
            Id = 12345
        });

        await DbContext.AddAsync(new Guild() { Id = "12345", Name = "Guild" });
        await DbContext.AddAsync(new GuildChannel() { Name = "Channel", GuildId = "12345", ChannelId = "12345" });
        await DbContext.AddAsync(new GuildUser() { GuildId = "12345", UserId = "12345" });
        await DbContext.AddAsync(new User() { Id = "12345", Username = "Username", Discriminator = "1234" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetFileContentAsync(12345, 123);
        CheckResult<NotFoundObjectResult>(result);
    }
}
