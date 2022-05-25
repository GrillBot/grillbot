using GrillBot.App.Controllers;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SystemControllerTests : ControllerTest<SystemController>
{
    protected override bool CanInitProvider() => false;

    protected override SystemController CreateController(IServiceProvider provider)
    {
        var environment = EnvironmentHelper.CreateEnv("Production");
        var client = DiscordHelper.CreateClient();
        var initManager = new InitManager(LoggingHelper.CreateLoggerFactory());

        return new SystemController(environment, client, initManager);
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
