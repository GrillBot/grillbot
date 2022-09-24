using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class InviteControllerTests : ControllerTest<InviteController>
{
    protected override InviteController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var inviteManager = new InviteManager(CacheBuilder, TestServices.CounterManager.Value);
        var service = new InviteService(discordClient, DatabaseBuilder, auditLogWriter, inviteManager);

        return new InviteController(service, ApiRequestContext, ServiceProvider);
    }

    [TestMethod]
    public async Task RefreshMetadataAsync()
    {
        var result = await Controller.RefreshMetadataCacheAsync();
        CheckResult<OkObjectResult, Dictionary<string, int>>(result);
    }

    [TestMethod]
    public async Task GetCurrentMetadataCount()
    {
        var result = await Controller.GetCurrentMetadataCountAsync();
        CheckResult<OkObjectResult, int>(result);

        Assert.AreEqual(0, ((OkObjectResult)result.Result)?.Value);
    }
}
