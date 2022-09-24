using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PermissionsControllerTests : ControllerTest<PermissionsController>
{
    protected override PermissionsController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        return new PermissionsController(discordClient, TestServices.AutoMapper.Value, DatabaseBuilder, ServiceProvider);
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_NotFound()
    {
        var result = await Controller.RemoveExplicitPermissionAsync("unverify", Consts.UserId.ToString());
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_Found()
    {
        var parameters = new CreateExplicitPermissionParams
        {
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = Consts.UserId.ToString()
        };

        CheckResult<OkResult>(await Controller.CreateExplicitPermissionAsync(parameters));
        CheckResult<OkResult>(await Controller.RemoveExplicitPermissionAsync("unverify", Consts.UserId.ToString()));
    }

    [TestMethod]
    public async Task GetExplicitPermissionsListAsync_WithoutFilter()
    {
        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.ExplicitPermission { Command = "unverify", IsRole = true, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = Consts.RoleId.ToString() },
            new Database.Entity.ExplicitPermission { Command = "unverify", IsRole = false, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = Consts.UserId.ToString() }
        });
        await Repository.AddAsync(new Database.Entity.User { Username = Consts.Username, Discriminator = Consts.Discriminator, Id = Consts.UserId.ToString() });
        await Repository.CommitAsync();

        var result = await Controller.GetExplicitPermissionsListAsync(null);
        CheckResult<OkObjectResult, List<ExplicitPermission>>(result);
    }

    [TestMethod]
    public async Task GetExplicitPermissionsListAsync_WithFilter()
    {
        var result = await Controller.GetExplicitPermissionsListAsync("selfunverify");
        CheckResult<OkObjectResult, List<ExplicitPermission>>(result);
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
