using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.MessageCache;
using GrillBot.App.Services.Reminder;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Cache;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class RemindControllerTests : ControllerTest<ReminderController>
{
    protected override bool CanInitProvider() => false;

    protected override ReminderController CreateController(IServiceProvider provider)
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var fileStorage = FileStorageHelper.Create(configuration);
        var initializationService = new DiscordInitializationService(LoggingHelper.CreateLogger<DiscordInitializationService>());
        var messageCache = new MessageCache(discordClient, initializationService, CacheBuilder);
        var auditLogService = new AuditLogService(discordClient, DbFactory, messageCache, fileStorage, initializationService);
        var remindService = new RemindService(discordClient, DbFactory, configuration, auditLogService);
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new RemindApiService(DbFactory, mapper);

        return new ReminderController(remindService, apiService);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test"
        });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetRemindMessagesListAsync(new GetReminderListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithFilter()
    {
        var filter = new GetReminderListParams()
        {
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            FromUserId = Consts.UserId.ToString(),
            MessageContains = "Test",
            OnlyWaiting = true,
            OriginalMessageId = Consts.MessageId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Sort = new()
            {
                OrderBy = "ToUser",
                Descending = true
            }
        };

        var result = await AdminController.GetRemindMessagesListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_NotFound()
    {
        var result = await AdminController.CancelRemindAsync(1, false);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_WasCancelled_Remind()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1,
            RemindMessageId = Consts.MessageId.ToString()
        });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.CancelRemindAsync(1, false);
        CheckResult<ObjectResult>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_Success()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1
        });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        await AdminController.CancelRemindAsync(1, false);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithoutFilter_AsUser()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1
        });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetRemindMessagesListAsync(new GetReminderListParams() { Sort = new() { Descending = true, OrderBy = "At" } }, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_InvalidSort_AsUser()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage()
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1
        });
        await DbContext.AddAsync(new Database.Entity.User() { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await DbContext.SaveChangesAsync();

        var result = await UserController.GetRemindMessagesListAsync(new GetReminderListParams() { Sort = new() { Descending = true, OrderBy = "ToUser" } }, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }
}
