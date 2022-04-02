using GrillBot.App.Controllers;
using GrillBot.App.Services.AutoReply;
using GrillBot.App.Services.Discord;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AutoReplyControllerTests : ControllerTest<AutoReplyController>
{
    protected override AutoReplyController CreateController()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var discordClient = DiscordHelper.CreateClient();
        var logger = LoggingHelper.CreateLogger<DiscordInitializationService>();
        var initializationService = new DiscordInitializationService(logger);
        var service = new AutoReplyService(configuration, discordClient, DbFactory, initializationService);

        return new AutoReplyController(service);
    }

    [TestMethod]
    public async Task CreateAndGetAutoReplyListAsync()
    {
        var createResult = await AdminController.CreateItemAsync(new AutoReplyItemParams()
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        }, CancellationToken.None);

        var listResult = await AdminController.GetAutoReplyListAsync(CancellationToken.None);

        CheckResult<OkObjectResult, AutoReplyItem>(createResult);
        CheckResult<OkObjectResult, List<AutoReplyItem>>(listResult);
    }

    [TestMethod]
    public async Task GetItemAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.AutoReplyItem()
        {
            Id = 1,
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetItemAsync(1, CancellationToken.None);
        CheckResult<OkObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task GetItemAsync_NotFound()
    {
        var result = await AdminController.GetItemAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task UpdateItemAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.AutoReplyItem()
        {
            Id = 1,
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.UpdateItemAsync(1, new AutoReplyItemParams()
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        }, CancellationToken.None);

        CheckResult<OkObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task UpdateItemAsync_NotFound()
    {
        var result = await AdminController.UpdateItemAsync(1, new AutoReplyItemParams()
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        }, CancellationToken.None);

        CheckResult<NotFoundObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_Found()
    {
        await DbContext.AddAsync(new Database.Entity.AutoReplyItem()
        {
            Id = 1,
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.RemoveItemAsync(1, CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_NotFound()
    {
        var result = await AdminController.RemoveItemAsync(1, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }
}
