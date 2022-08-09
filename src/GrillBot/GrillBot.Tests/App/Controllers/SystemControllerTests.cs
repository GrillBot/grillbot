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
        var environment = EnvironmentHelper.CreateEnv("Production");
        var client = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());

        return new SystemController(environment, client, initManager, TestServices.CounterManager.Value);
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
}
