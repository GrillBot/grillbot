using GrillBot.Cache.Services.Repository;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using GrillBot.Database.Services.Repository;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.Common;

public abstract class ControllerTest : TestBase
{
    protected static readonly Lazy<ApiRequestContext> UserApiRequestContext
        = new(() => CreateApiRequestContext("User"), LazyThreadSafetyMode.ExecutionAndPublication);

    protected static readonly Lazy<ApiRequestContext> AdminApiRequestContext
        = new(() => CreateApiRequestContext("Admin"), LazyThreadSafetyMode.ExecutionAndPublication);

    private ControllerTestConfiguration TestConfiguration
        => GetMethod().GetCustomAttribute<ControllerTestConfiguration>();

    protected bool IsPublic() => TestConfiguration?.IsPublic ?? false;
    protected bool CanInitProvider() => TestConfiguration?.CanInitProvider ?? false;

    private static ApiRequestContext CreateApiRequestContext(string role)
    {
        return new ApiRequestContext
        {
            LoggedUser = new UserBuilder().SetIdentity(Consts.UserId, Consts.Username + "-" + role, Consts.Discriminator).Build(),
            LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Consts.UserId.ToString())
            }))
        };
    }
}

[ExcludeFromCodeCoverage]
public abstract class ControllerTest<TController> : ControllerTest where TController : Controller
{
    protected TController Controller { get; private set; }
    protected ApiRequestContext ApiRequestContext { get; private set; }
    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }
    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }
    protected IServiceProvider ServiceProvider { get; private set; }

    private static bool ContainsApiRequestContext
        => typeof(TController).GetProperty("ApiRequestContext", BindingFlags.Instance | BindingFlags.NonPublic) != null;

    protected abstract TController CreateController();

    [TestInitialize]
    public void TestInitialization()
    {
        DatabaseBuilder = TestServices.DatabaseBuilder.Value;
        CacheBuilder = TestServices.CacheBuilder.Value;
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();
        ServiceProvider = CreateProvider(CanInitProvider());

        ApiRequestContext = IsPublic() ? UserApiRequestContext.Value : AdminApiRequestContext.Value;
        Controller = CreateController();
        Controller.ControllerContext = CreateContext(ApiRequestContext);
        if (ContainsApiRequestContext)
            ReflectionHelper.SetPrivateReadonlyPropertyValue(Controller, nameof(ApiRequestContext), ApiRequestContext);
    }

    protected virtual void Cleanup()
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
        Controller.Dispose();
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
        return init ? TestServices.InitializedProvider.Value : TestServices.EmptyProvider.Value;
    }
}
