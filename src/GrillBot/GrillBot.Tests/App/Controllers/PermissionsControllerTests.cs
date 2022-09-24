using GrillBot.App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PermissionsControllerTests : ControllerTest<PermissionsController>
{
    protected override PermissionsController CreateController()
    {
        return new PermissionsController(DatabaseBuilder, ServiceProvider);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_Found()
    {
        await Repository.AddAsync(new Database.Entity.ExplicitPermission
        {
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Banned,
            TargetId = Consts.UserId.ToString()
        });
        await Repository.CommitAsync();

        var result = await Controller.SetExplicitPermissionStateAsync("unverify", Consts.UserId.ToString(), Database.Enums.ExplicitPermissionState.Allowed);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_NotFound()
    {
        var result = await Controller.SetExplicitPermissionStateAsync("unverify", Consts.UserId.ToString(), Database.Enums.ExplicitPermissionState.Allowed);
        CheckResult<NotFoundObjectResult>(result);
    }
}
