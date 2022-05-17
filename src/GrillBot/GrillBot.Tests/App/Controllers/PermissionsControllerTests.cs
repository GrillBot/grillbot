using GrillBot.App.Controllers;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class PermissionsControllerTests : ControllerTest<PermissionsController>
{
    protected override bool CanInitProvider() => false;

    protected override PermissionsController CreateController(IServiceProvider provider)
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
            TargetId = Consts.UserId.ToString()
        };

        var result = await AdminController.CreateExplicitPermissionAsync(parameters);
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
            TargetId = Consts.UserId.ToString()
        };

        CheckResult<OkResult>(await AdminController.CreateExplicitPermissionAsync(parameters));
        CheckResult<ConflictObjectResult>(await AdminController.CreateExplicitPermissionAsync(parameters));
    }

    [TestMethod]
    public async Task RemoveExplicitPermissionAsync_NotFound()
    {
        var result = await AdminController.RemoveExplicitPermissionAsync("unverify", Consts.UserId.ToString());
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
            TargetId = Consts.UserId.ToString()
        };

        CheckResult<OkResult>(await AdminController.CreateExplicitPermissionAsync(parameters));
        CheckResult<OkResult>(await AdminController.RemoveExplicitPermissionAsync("unverify", Consts.UserId.ToString()));
    }

    [TestMethod]
    public async Task GetExplicitPermissionsListAsync_WithoutFilter()
    {
        await DbContext.AddRangeAsync(new[]
        {
            new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = true, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = Consts.RoleId.ToString() },
            new Database.Entity.ExplicitPermission() { Command = "unverify", IsRole = false, State = Database.Enums.ExplicitPermissionState.Banned, TargetId = Consts.UserId.ToString() }
        });
        await DbContext.AddAsync(new Database.Entity.User() { Username = Consts.Username, Discriminator = Consts.Discriminator, Id = Consts.UserId.ToString() });
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
        await DbContext.AddAsync(new Database.Entity.ExplicitPermission()
        {
            Command = "unverify",
            IsRole = false,
            State = Database.Enums.ExplicitPermissionState.Banned,
            TargetId = Consts.UserId.ToString()
        });
        await DbContext.SaveChangesAsync();

        var result = await AdminController.SetExplicitPermissionStateAsync("unverify", Consts.UserId.ToString(), Database.Enums.ExplicitPermissionState.Allowed);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task SetExplicitPermissionState_NotFound()
    {
        var result = await AdminController.SetExplicitPermissionStateAsync("unverify", Consts.UserId.ToString(), Database.Enums.ExplicitPermissionState.Allowed);
        CheckResult<NotFoundObjectResult>(result);
    }
}
