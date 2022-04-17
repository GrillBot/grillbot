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
        var mapper = AutoMapperHelper.CreateMapper();
        return new PermissionsController(DbContext, discordClient, mapper);
    }

    [TestMethod]
    public async Task CreateExplicitPermissionAsync_NotExists()
    {
        var parameters = new CreateExplicitPermissionParams()
        {
            Command = "$unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = "12345"
        };

        var result = await AdminController.CreateExplicitPermissionAsync(parameters, CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task CreateExplicitPermissionAsync_Exists()
    {
        var parameters = new CreateExplicitPermissionParams()
        {
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = "12345"
        };

        CheckResult<OkResult>(await AdminController.CreateExplicitPermissionAsync(parameters, CancellationToken.None));
        CheckResult<ConflictObjectResult>(await AdminController.CreateExplicitPermissionAsync(parameters, CancellationToken.None));
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_NotFound()
    {
        var result = await AdminController.RemoveExplicitPermissionAsync("unverify", "12345", CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_Found()
    {
        var parameters = new CreateExplicitPermissionParams()
        {
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = "12345"
        };

        CheckResult<OkResult>(await AdminController.CreateExplicitPermissionAsync(parameters, CancellationToken.None));
        CheckResult<OkResult>(await AdminController.RemoveExplicitPermissionAsync("unverify", "12345", CancellationToken.None));
    }

    [TestMethod]
    public async Task GetExplicitPermissionsListAsync_WithoutFilter()
    {
        await DbContext.AddRangeAsync(new[]
        {
            new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = true, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = "12345" },
            new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = false, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = "123456" }
        });
        await DbContext.AddAsync(new Database.Entity.User() { Username = "User", Discriminator = "1234", Id = "123456" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.GetExplicitPermissionsListAsync(null, CancellationToken.None);
        CheckResult<OkObjectResult, List<ExplicitPermission>>(result);
    }

    [TestMethod]
    public async Task GetExplicitPermissionsListAsync_WithFilter()
    {
        var result = await AdminController.GetExplicitPermissionsListAsync("selfunverify", CancellationToken.None);
        CheckResult<OkObjectResult, List<ExplicitPermission>>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_Found()
    {
        await DbContext.AddAsync(new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = false, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.SetExplicitPermissionStateAsync("unverify", "12345", Database.Enums.ExplicitPermissionState.Allowed, CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_NotFound()
    {
        var result = await AdminController.SetExplicitPermissionStateAsync("unverify", "12345", Database.Enums.ExplicitPermissionState.Allowed, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }
}
