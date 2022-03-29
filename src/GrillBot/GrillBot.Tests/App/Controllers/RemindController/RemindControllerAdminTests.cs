using GrillBot.App.Controllers;
using GrillBot.App.Services.Reminder;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Reminder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class RemindControllerAdminTests : ControllerTest<ReminderController>
{
    protected override ReminderController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var configuration = ConfigurationHelper.CreateConfiguration();
        var remindService = new RemindService(discordClient, DbFactory, configuration);

        return new ReminderController(DbContext, remindService)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                        new Claim(ClaimTypes.Role, "Admin")
                    }))
                }
            }
        };
    }

    public override void Cleanup()
    {
        DbContext.RemoveRange(DbContext.Reminders.AsEnumerable());
        DbContext.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithoutFilter()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage() { At = DateTime.Now, FromUserId = "12345", ToUserId = "12345", Message = "Test" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "User", Discriminator = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams(), CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_WithFilter()
    {
        var filter = new GetReminderListParams()
        {
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            FromUserId = "12345",
            MessageContains = "Test",
            OnlyWaiting = true,
            OriginalMessageId = "12345",
            ToUserId = "12345",
            SortBy = "touser",
            SortDesc = true
        };

        var result = await Controller.GetRemindMessagesListAsync(filter, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_NotFound()
    {
        var result = await Controller.CancelRemindAsync(1, false);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task CancelRemindAsync_WasCancelled_Remind()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage() { At = DateTime.MaxValue, FromUserId = "12345", ToUserId = "12345", Message = "Test", Id = 1, RemindMessageId = "1" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "User", Discriminator = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.CancelRemindAsync(1, false);
        CheckResult<ObjectResult>(result);
    }

    [TestMethod]
    [ExpectedException(typeof(NullReferenceException))] // NullReference is here correct, because cannot get discord user without connection.
    [ExcludeFromCodeCoverage]
    public async Task CancelRemindAsync_Success()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage() { At = DateTime.MaxValue, FromUserId = "12345", ToUserId = "12345", Message = "Test", Id = 1 });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "User", Discriminator = "12345" });
        await DbContext.SaveChangesAsync();

        await Controller.CancelRemindAsync(1, false);
    }
}
