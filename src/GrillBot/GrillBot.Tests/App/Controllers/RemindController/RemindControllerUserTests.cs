using GrillBot.App.Controllers;
using GrillBot.App.Services.Reminder;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Reminder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class RemindControllerUserTests : ControllerTest<ReminderController>
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
                        new Claim(ClaimTypes.Role, "User")
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

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams() { SortDesc = true, SortBy = "at" }, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }

    [TestMethod]
    public async Task GetRemindMessagesListAsync_InvalidSort()
    {
        await DbContext.AddAsync(new Database.Entity.RemindMessage() { At = DateTime.Now, FromUserId = "12345", ToUserId = "12345", Message = "Test" });
        await DbContext.AddAsync(new Database.Entity.User() { Id = "12345", Username = "User", Discriminator = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetRemindMessagesListAsync(new GetReminderListParams() { SortDesc = true, SortBy = "ToUser" }, CancellationToken.None);
        CheckResult<OkObjectResult, PaginatedResponse<RemindMessage>>(result);
    }
}
