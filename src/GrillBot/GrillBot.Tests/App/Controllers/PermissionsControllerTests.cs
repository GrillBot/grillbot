using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PermissionsControllerTests : ControllerTest<PermissionsController>
{
    protected override PermissionsController CreateController()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();
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
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Allowed,
            TargetId = "12345"
        };

        var result = await Controller.CreateExplicitPermissionAsync(parameters);
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

        CheckResult<OkResult>(await Controller.CreateExplicitPermissionAsync(parameters));
        CheckResult<ConflictObjectResult>(await Controller.CreateExplicitPermissionAsync(parameters));
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_NotFound()
    {
        var result = await Controller.RemoveExplicitPermissionAsync("unverify", "12345");
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

        CheckResult<OkResult>(await Controller.CreateExplicitPermissionAsync(parameters));
        CheckResult<OkResult>(await Controller.RemoveExplicitPermissionAsync("unverify", "12345"));
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
        await DbContext.AddAsync(new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = false, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = "12345" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.SetExplicitPermissionStateAsync("unverify", "12345", Database.Enums.ExplicitPermissionState.Allowed);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_NotFound()
    {
        var result = await Controller.SetExplicitPermissionStateAsync("unverify", "12345", Database.Enums.ExplicitPermissionState.Allowed);
        CheckResult<NotFoundObjectResult>(result);
    }
}
