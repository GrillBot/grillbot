using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Reminder;
using GrillBot.Data.Models.API.Reminder;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Database.Models;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class RemindControllerTests : ControllerTest<ReminderController>
{
    protected override ReminderController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var texts = new TextsBuilder().Build();
        var remindService = new RemindService(discordClient, DatabaseBuilder, TestServices.Configuration.Value, texts);
        var apiService = new RemindApiService(DatabaseBuilder, TestServices.AutoMapper.Value, ApiRequestContext, remindService, auditLogWriter);

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

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams());
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
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

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams { Sort = { OrderBy = "ToUser" } });
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

        var result = await Controller.GetRemindMessagesListAsync(filter);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_NotFound()
    {
        var result = await Controller.CancelRemindAsync(1);
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

        var result = await Controller.CancelRemindAsync(1);
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

        await Controller.CancelRemindAsync(1);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
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

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams { Sort = new SortParams { Descending = true, OrderBy = "At" } });
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
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

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams { Sort = new SortParams { Descending = true, OrderBy = "ToUser" } });
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }
}
