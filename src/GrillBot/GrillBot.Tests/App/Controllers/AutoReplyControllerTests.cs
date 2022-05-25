using GrillBot.App.Controllers;
using GrillBot.App.Services.AutoReply;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AutoReplyControllerTests : ControllerTest<AutoReplyController>
{
    protected override bool CanInitProvider() => false;

    protected override AutoReplyController CreateController(IServiceProvider provider)
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var discordClient = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var service = new AutoReplyService(configuration, discordClient, DbFactory, initManager);
        var mapper = AutoMapperHelper.CreateMapper();
        var apiService = new AutoReplyApiService(service, DbFactory, mapper);

        return new AutoReplyController(apiService);
    }

    [TestMethod]
    public async Task CreateAndGetAutoReplyListAsync()
    {
        var createResult = await AdminController.CreateItemAsync(new AutoReplyItemParams()
        {
            Flags = 2,
            Reply = "Reply",
            Template = "Template"
        });

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
        });

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
        });

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

        var result = await AdminController.RemoveItemAsync(1);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task RemoveItemAsync_NotFound()
    {
        var result = await AdminController.RemoveItemAsync(1);
        CheckResult<NotFoundObjectResult>(result);
    }
}
