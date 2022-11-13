using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using System.Reflection;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Tests.App.Infrastructure.Auth;

[TestClass]
public class ApiKeyAuthAttributeTests
{
    private bool _wasCalled;
    
    private GrillBotDatabaseBuilder DatabaseBuilder { get; set; }

    private Task<ActionExecutedContext> Delegate()
    {
        _wasCalled = true;
        return Task.FromResult<ActionExecutedContext>(null);
    }

    [TestInitialize]
    public void OnInitialize()
    {
        _wasCalled = false;
    }

    [TestCleanup]
    public void OnCleanup()
    {
        TestDatabaseBuilder.ClearDatabase();
        TestCacheBuilder.ClearDatabase();
    }

    private static ActionExecutingContext CreateContext(IHeaderDictionary headers)
    {
        var request = new Mock<HttpRequest>();
        request.Setup(o => o.Headers).Returns(headers);

        var serviceProvider = TestServices.InitializedProvider.Value;
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(o => o.Request).Returns(request.Object);
        httpContext.Setup(o => o.RequestServices).Returns(serviceProvider);

        var actionContext = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
        var controller = new AuthController(null);

        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), controller)
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = typeof(AuthController).GetTypeInfo(),
                MethodInfo = typeof(AuthController).GetMethod("GetRedirectLink")!
            }
        };
    }

    private async Task InitDataAsync(ActionContext context)
    {
        DatabaseBuilder = context.HttpContext.RequestServices.GetRequiredService<GrillBotDatabaseBuilder>();
        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.AddCollectionAsync(new[]
        {
            new ApiClient { Id = "963258740" },
            new ApiClient { Id = "963258741", AllowedMethods = new List<string> { "AuthController.GetRedirectLink" } },
            new ApiClient { Id = "963258742", AllowedMethods = new List<string> { "*" } },
            new ApiClient { Id = "963258743", AllowedMethods = new List<string> { "GetLink" } },
        });
        await repository.CommitAsync();
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_MissingHeader()
    {
        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(new HeaderDictionary());

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_InvalidAuthorizationHeader()
    {
        var headers = new HeaderDictionary { { "Authorization", "Test 123" } };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_MissingClient()
    {
        var headers = new HeaderDictionary { { "ApiKey", "12345" } };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_NoAllowedMethods()
    {
        var headers = new HeaderDictionary { { "Authorization", "ApiKey 963258740" } };
        var context = CreateContext(headers);
        await InitDataAsync(context);

        var attribute = new ApiKeyAuthAttribute();
        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_AllowAll()
    {
        var headers = new HeaderDictionary { { "ApiKey", "963258742" } };
        var context = CreateContext(headers);
        await InitDataAsync(context);

        var attribute = new ApiKeyAuthAttribute();
        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsTrue(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_UnallowedMethod()
    {
        var headers = new HeaderDictionary { { "ApiKey", "963258743" } };
        var context = CreateContext(headers);
        await InitDataAsync(context);

        var attribute = new ApiKeyAuthAttribute();
        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_Success()
    {
        var headers = new HeaderDictionary { { "ApiKey", "963258741" } };
        var context = CreateContext(headers);
        await InitDataAsync(context);

        var attribute = new ApiKeyAuthAttribute();
        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsTrue(_wasCalled);
    }
}
