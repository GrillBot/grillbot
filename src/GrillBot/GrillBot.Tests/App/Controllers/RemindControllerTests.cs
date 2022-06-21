using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Reminder;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class RemindControllerTests : ControllerTest<ReminderController>
{
    protected override bool CanInitProvider() => false;

    protected override ReminderController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var remindService = new RemindService(discordClient, DatabaseBuilder, configuration);
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new RemindApiService(DatabaseBuilder, mapper, ApiRequestContext, remindService, auditLogWriter);

        return new ReminderController(apiService, ApiRequestContext);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithoutFilter()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test"
        });
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await AdminController.GetRemindMessagesListAsync(new GetReminderListParams());
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_AsUser_WithoutFilter()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test"
        });
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        SelectApiRequestContext(true);
        ReflectionHelper.SetPrivateReadonlyPropertyValue(UserController, nameof(ApiRequestContext), ApiRequestContext);

        var result = await UserController.GetRemindMessagesListAsync(new GetReminderListParams { Sort = { OrderBy = "ToUser" } });
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithFilter()
    {
        var filter = new GetReminderListParams
        {
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            FromUserId = Consts.UserId.ToString(),
            MessageContains = "Test",
            OnlyWaiting = true,
            OriginalMessageId = Consts.MessageId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Sort = new SortParams
            {
                OrderBy = "ToUser",
                Descending = true
            }
        };

        var result = await AdminController.GetRemindMessagesListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_NotFound()
    {
        var result = await AdminController.CancelRemindAsync(1);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_WasCancelled_Remind()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1,
            RemindMessageId = Consts.MessageId.ToString()
        });
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await AdminController.CancelRemindAsync(1);
        CheckResult<ObjectResult>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_Success()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1
        });
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        await AdminController.CancelRemindAsync(1);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithoutFilter_AsUser()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1
        });
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await UserController.GetRemindMessagesListAsync(new GetReminderListParams { Sort = new SortParams { Descending = true, OrderBy = "At" } });
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_InvalidSort_AsUser()
    {
        await Repository.AddAsync(new Database.Entity.RemindMessage
        {
            At = DateTime.Now,
            FromUserId = Consts.UserId.ToString(),
            ToUserId = Consts.UserId.ToString(),
            Message = "Test",
            Id = 1
        });
        await Repository.AddAsync(new Database.Entity.User { Id = Consts.UserId.ToString(), Username = Consts.Username, Discriminator = Consts.Discriminator });
        await Repository.CommitAsync();

        var result = await UserController.GetRemindMessagesListAsync(new GetReminderListParams { Sort = new SortParams { Descending = true, OrderBy = "ToUser" } });
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }
}
