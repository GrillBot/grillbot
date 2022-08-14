using GrillBot.App.Controllers;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SystemControllerTests : ControllerTest<SystemController>
{
    protected override SystemController CreateController()
    {
        var client = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());
        var interactionService = DiscordHelper.CreateInteractionService(client);
        var commandService = DiscordHelper.CreateCommandsService();
        var eventManager = new EventManager(client, interactionService, commandService);

        return new SystemController(TestServices.ProductionEnvironment.Value, client, initManager, TestServices.CounterManager.Value, eventManager);
    }

    [TestMethod]
    public void GetDiagnostics()
    {
        var result = Controller.GetDiagnostics();
        CheckResult<OkObjectResult, DiagnosticsInfo>(result);
    }

    [TestMethod]
    public void ChangeBotStatus()
    {
        var result = Controller.ChangeBotStatus(true);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public void GetEventLog()
    {
        var result = Controller.GetEventLog();
        CheckResult<OkObjectResult, string[]>(result);
    }
}
