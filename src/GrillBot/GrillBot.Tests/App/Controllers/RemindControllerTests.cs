using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Reminder;
using Microsoft.AspNetCore.Mvc;

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
        var apiService = new RemindApiService(DatabaseBuilder, ApiRequestContext, remindService, auditLogWriter);

        return new ReminderController(apiService, ServiceProvider);
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
}
