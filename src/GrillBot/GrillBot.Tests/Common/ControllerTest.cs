using GrillBot.Data.Models.API;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ControllerTest<TController> where TController : Controller
{
    protected TController AdminController { get; private set; }
    protected TController UserController { get; private set; }

    protected GrillBotContext DbContext { get; set; }
    protected GrillBotContextFactory DbFactory { get; set; }

    protected abstract TController CreateController();

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        AdminController = CreateController();
        AdminController.ControllerContext = CreateContext("Admin");

        UserController = CreateController();
        UserController.ControllerContext = CreateContext("User");
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        DbContext.ChangeTracker.Clear();
        DatabaseHelper.ClearDatabase(DbContext);

        Cleanup();

        DbContext.Dispose();
        AdminController.Dispose();
        UserController.Dispose();
    }

    private static ControllerContext CreateContext(string role)
    {
        return new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
                }))
            }
        };
    }

    protected void CheckResult<TResult>(IActionResult result) where TResult : IActionResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(TResult));

        if (result is NotFoundObjectResult notFound)
        {
            Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
        }
        else if (result is FileContentResult fileContent)
        {
            Assert.IsNotNull(fileContent.FileContents);
            Assert.IsTrue(fileContent.FileContents.Length > 0);
        }
    }

    protected void CheckResult<TResult, TOkModel>(ActionResult<TOkModel> result) where TResult : ObjectResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(TResult));

        if (result.Result is OkObjectResult ok)
            Assert.IsInstanceOfType(ok.Value, typeof(TOkModel));
        else if (result.Result is NotFoundObjectResult notFound)
            Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
        else if (result.Result is BadRequestObjectResult badRequest)
            Assert.IsInstanceOfType(badRequest.Value, typeof(ValidationProblemDetails));
    }
}
