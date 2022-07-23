using GrillBot.Cache.Services.Repository;
using GrillBot.Data.Models.API;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using GrillBot.Common.Models;
using GrillBot.Database.Services.Repository;
using GrillBot.Tests.Infrastructure.Database;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ControllerTest<TController> where TController : Controller
{
    protected TController AdminController { get; private set; }
    protected TController UserController { get; private set; }

    private ApiRequestContext UserApiRequestContext { get; set; }
    private ApiRequestContext AdminApiRequestContext { get; set; }
    protected ApiRequestContext ApiRequestContext { get; private set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected IServiceProvider ServiceProvider { get; private set; }

    protected abstract bool CanInitProvider();
    protected abstract TController CreateController();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = new TestDatabaseBuilder();
        CacheBuilder = new TestCacheBuilder();
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();
        UserApiRequestContext = CreateApiRequestContext("User");
        AdminApiRequestContext = CreateApiRequestContext("Admin");
        SelectApiRequestContext(false);
        ServiceProvider = CreateProvider(CanInitProvider());

        AdminController = CreateController();
        AdminController.ControllerContext = CreateContext(AdminApiRequestContext);

        UserController = CreateController();
        UserController.ControllerContext = CreateContext(UserApiRequestContext);
    }

    public virtual void Cleanup()
    {
    }

    [TestCleanup]
    public void TestClean()
    {
        Cleanup();

        TestDatabaseBuilder.ClearDatabase();
        TestCacheBuilder.ClearDatabase();
        Repository.Dispose();
        CacheRepository.Dispose();
        UserController.Dispose();
        AdminController.Dispose();
    }

    private static ApiRequestContext CreateApiRequestContext(string role)
    {
        return new ApiRequestContext
        {
            LoggedUser = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build(),
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            }))
        };
    }

    private ControllerContext CreateContext(ApiRequestContext context)
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = context.LoggedUserData!,
                RequestServices = ServiceProvider
            }
        };
    }

    protected void CheckResult<TResult>(IActionResult result) where TResult : IActionResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(TResult));

        switch (result)
        {
            case NotFoundObjectResult notFound:
                Assert.IsNotNull(notFound.Value);
                Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
                break;
            case FileContentResult fileContent:
                Assert.IsNotNull(fileContent.FileContents);
                Assert.IsTrue(fileContent.FileContents.Length > 0);
                break;
        }
    }

    protected void CheckResult<TResult, TOkModel>(ActionResult<TOkModel> result) where TResult : ObjectResult
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(TResult));

        switch (result.Result)
        {
            case OkObjectResult ok:
                Assert.IsNotNull(ok.Value);
                Assert.IsInstanceOfType(ok.Value, typeof(TOkModel));
                break;
            case NotFoundObjectResult notFound:
                Assert.IsNotNull(notFound.Value);
                Assert.IsInstanceOfType(notFound.Value, typeof(MessageResponse));
                break;
            case BadRequestObjectResult badRequest:
                Assert.IsNotNull(badRequest.Value);
                Assert.IsInstanceOfType(badRequest.Value, typeof(ValidationProblemDetails));
                break;
        }
    }

    protected void CheckForStatusCode(IActionResult result, int statusCode)
    {
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(ObjectResult));

        var statusCodeResult = (ObjectResult)result;
        Assert.IsNotNull(statusCodeResult.StatusCode);
        Assert.AreEqual(statusCode, statusCodeResult.StatusCode.Value);
    }

    private static IServiceProvider CreateProvider(bool init = false)
    {
        return init ? DiHelper.CreateInitializedProvider() : DiHelper.CreateEmptyProvider();
    }

    protected void SelectApiRequestContext(bool selectPublic)
    {
        ApiRequestContext = selectPublic ? UserApiRequestContext : AdminApiRequestContext;
    }
}
