using GrillBot.App.Controllers;
using GrillBot.App.Services;
using GrillBot.App.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class SearchingControllerTests : ControllerTest<SearchingController>
{
    protected override SearchingController CreateController()
    {
        var userService = new UserService(DatabaseBuilder, TestServices.Configuration.Value);
        var searchingService = new SearchingService(DatabaseBuilder, userService, ServiceProvider);

        return new SearchingController(searchingService, ServiceProvider);
    }

    [TestMethod]
    public async Task RemoveSearchesAsync()
    {
        var result = await Controller.RemoveSearchesAsync(new[] { 1L });
        CheckResult<OkResult>(result);
    }
}
