using GrillBot.App.Controllers;
using GrillBot.App.Services.Discord;
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
        var logger = LoggingHelper.CreateLogger<DiscordInitializationService>();
        var initialization = new DiscordInitializationService(logger);

        return new SystemController(environment, client, initialization);
    }

    [TestMethod]
    public void GetDiagnostics()
    {
        var result = AdminController.GetDiagnostics();
        CheckResult<OkObjectResult, DiagnosticsInfo>(result);
    }

    [TestMethod]
    public void ChangeBotStatus()
    {
        var result = AdminController.ChangeBotStatus(true);
        CheckResult<OkResult>(result);
    }
}
