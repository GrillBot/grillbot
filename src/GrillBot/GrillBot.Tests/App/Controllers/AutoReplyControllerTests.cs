using GrillBot.App.Controllers;
using GrillBot.App.Services.AutoReply;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AutoReplyControllerTests : ControllerTest<AutoReplyController>
{
    protected override AutoReplyController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var service = new AutoReplyService(TestServices.Configuration.Value, discordClient, DatabaseBuilder, initManager);
        var apiService = new AutoReplyApiService(service, DatabaseBuilder, TestServices.AutoMapper.Value);

        return new AutoReplyController(apiService);
    }

    [TestMethod]
    public async Task CreateAndGetAutoReplyListAsync()
    {
        var createResult = await Controller.CreateItemAsync(new AutoReplyItemParams
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });

        var listResult = await Controller.GetAutoReplyListAsync();

        CheckResult<OkObjectResult, AutoReplyItem>(createResult);
        CheckResult<OkObjectResult, List<AutoReplyItem>>(listResult);
    }

    [TestMethod]
    public async Task GetItemAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem
        {
            Id = 1,
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });
        await Repository.CommitAsync();

        var result = await Controller.GetItemAsync(1);
        CheckResult<OkObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task GetItemAsync_NotFound()
    {
        var result = await Controller.GetItemAsync(1);
        CheckResult<NotFoundObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task UpdateItemAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem
        {
            Id = 1,
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });
        await Repository.CommitAsync();

        var result = await Controller.UpdateItemAsync(1, new AutoReplyItemParams
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });

        CheckResult<OkObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task UpdateItemAsync_NotFound()
    {
        var result = await Controller.UpdateItemAsync(1, new AutoReplyItemParams
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });

        CheckResult<NotFoundObjectResult, AutoReplyItem>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_Found()
    {
        await Repository.AddAsync(new Database.Entity.AutoReplyItem
        {
            Id = 1,
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });
        await Repository.CommitAsync();

        var result = await Controller.RemoveItemAsync(1);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_NotFound()
    {
        var result = await Controller.RemoveItemAsync(1);
        CheckResult<NotFoundObjectResult>(result);
    }
}
