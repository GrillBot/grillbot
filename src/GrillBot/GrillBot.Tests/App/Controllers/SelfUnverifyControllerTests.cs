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
        var service = new SelfunverifyService(null, DbFactory);
        return new SelfUnverifyController(service);
    }

    public override void Cleanup()
    {
        DbContext.SelfunverifyKeepables.RemoveRange(DbContext.SelfunverifyKeepables.AsQueryable());
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task AddKeepablesAsync_NotExists()
    {
        var parameters = new List<KeepableParams>()
        {
            new KeepableParams() { Group = "1BIT", Name = "IZP" },
            new KeepableParams() { Group = "2BIT", Name = "IAL" }
        };

        var result = await Controller.AddKeepableAsync(parameters, CancellationToken.None);
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task AddKeepablesAsync_Exists()
    {
        var parameters = new List<KeepableParams>()
        {
            new KeepableParams() { Group = "1BIT", Name = "IZP" },
        };

        await DbContext.SelfunverifyKeepables.AddAsync(new Database.Entity.SelfunverifyKeepable() { GroupName = "1bit", Name = "izp" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.AddKeepableAsync(parameters, CancellationToken.None);
        CheckResult<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task KeepableExistsAsync()
    {
        var parameter = new KeepableParams() { Group = "1BIT", Name = "IZP" };
        var result = await Controller.KeepableExistsAsync(parameter, CancellationToken.None);

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
        await DbContext.SelfunverifyKeepables.AddAsync(new Database.Entity.SelfunverifyKeepable() { GroupName = "1bit", Name = "izp" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.KeepableRemoveAsync("1bit", "izp");
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task KeepableRemoveAsync_Exists_Group()
    {
        await DbContext.SelfunverifyKeepables.AddAsync(new Database.Entity.SelfunverifyKeepable() { GroupName = "1bit", Name = "izp" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.KeepableRemoveAsync("1bit");
        CheckResult<OkResult>(result);
    }

    [TestMethod]
    public async Task KeepableRemoveAsync_NotExists_Group()
    {
        var result = await Controller.KeepableRemoveAsync("1bit");
        CheckResult<BadRequestObjectResult>(result);
    }

    [TestMethod]
    public async Task GetKeepablesAsync()
    {
        await DbContext.SelfunverifyKeepables.AddAsync(new Database.Entity.SelfunverifyKeepable() { GroupName = "1bit", Name = "izp" });
        await DbContext.SaveChangesAsync();

        var result = await Controller.GetKeepablesListAsync(CancellationToken.None);
        CheckResult<OkObjectResult, Dictionary<string, List<string>>>(result);
    }
}
