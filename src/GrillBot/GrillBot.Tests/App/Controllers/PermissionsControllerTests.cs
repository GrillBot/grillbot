using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Permissions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PermissionsControllerTests : ControllerTest<PermissionsController>
{
    protected override PermissionsController CreateController()
    {
        var discordClient = DiscordHelper.CreateClient();
        return new PermissionsController(DbContext, discordClient);
    }

    public override void Cleanup()
    {
        DbContext.ExplicitPermissions.RemoveRange(DbContext.ExplicitPermissions.AsEnumerable());
        DbContext.Users.RemoveRange(DbContext.Users.AsEnumerable());
        DbContext.SaveChanges();
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

        var result = await Controller.CreateExplicitPermissionAsync(parameters, CancellationToken.None);
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

        CheckResult<OkResult>(await Controller.CreateExplicitPermissionAsync(parameters, CancellationToken.None));
        CheckResult<ConflictObjectResult>(await Controller.CreateExplicitPermissionAsync(parameters, CancellationToken.None));
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_NotFound()
    {
        var result = await Controller.RemoveExplicitPermissionAsync("unverify", "12345", CancellationToken.None);
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

        CheckResult<OkResult>(await Controller.CreateExplicitPermissionAsync(parameters, CancellationToken.None));
        CheckResult<OkResult>(await Controller.RemoveExplicitPermissionAsync("unverify", "12345", CancellationToken.None));
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

        var result = await Controller.GetExplicitPermissionsListAsync(null, CancellationToken.None);
        CheckResult<OkObjectResult, List<ExplicitPermission>>(result);
    }

    [TestMethod]
    public async Task GetExplicitPermissionsListAsync_WithFilter()
    {
        var result = await Controller.GetExplicitPermissionsListAsync("selfunverify", CancellationToken.None);
        CheckResult<OkObjectResult, List<ExplicitPermission>>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_Found()
    {
        await DbContext.AddAsync(new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = false, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.SetExplicitPermissionStateAsync("unverify", "12345", Database.Enums.ExplicitPermissionState.Allowed, CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_NotFound()
    {
        var result = await Controller.SetExplicitPermissionStateAsync("unverify", "12345", Database.Enums.ExplicitPermissionState.Allowed, CancellationToken.None);
        CheckResult<NotFoundObjectResult>(result);
    }
}
