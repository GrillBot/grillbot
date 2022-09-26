using GrillBot.App.Controllers;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.API.Selfunverify;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SelfUnverifyControllerTests : ControllerTest<SelfUnverifyController>
{
    protected override SelfUnverifyController CreateController()
    {
        var service = new SelfunverifyService(null, DatabaseBuilder);
        return new SelfUnverifyController(service, ServiceProvider);
    }

    [TestMethod]
    public async Task KeepableExistsAsync()
    {
        var parameter = new KeepableParams { Group = "1BIT", Name = "IZP" };
        var result = await Controller.KeepableExistsAsync(parameter);

        CheckResult<OkObjectResult, bool>(result);
    }

    [TestMethod]
    public async Task KeepableRemoveAsync_NotExists()
    {
        var result = await Controller.KeepableRemoveAsync("1bit", "izp");
        CheckResult<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task KeepableRemoveAsync_Exists()
    {
        await Repository.AddAsync(new Database.Entity.SelfunverifyKeepable { GroupName = "1bit", Name = "izp" });
        await Repository.CommitAsync();

        var result = await Controller.KeepableRemoveAsync("1bit", "izp");
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task KeepableRemoveAsync_Exists_Group()
    {
        await Repository.AddAsync(new Database.Entity.SelfunverifyKeepable { GroupName = "1bit", Name = "izp" });
        await Repository.CommitAsync();

        var result = await Controller.KeepableRemoveAsync("1bit");
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task KeepableRemoveAsync_NotExists_Group()
    {
        var result = await Controller.KeepableRemoveAsync("1bit");
        CheckResult<BadRequestObjectResult>(result);
    }
}
