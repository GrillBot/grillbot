using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using System.Reflection;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace GrillBot.Tests.App.Infrastructure.Auth;

[TestClass]
public class ApiKeyAuthAttributeTests : TestBase<ApiKeyAuthAttribute>
{
    private bool _wasCalled;

    protected override ApiKeyAuthAttribute CreateInstance()
    {
        return new ApiKeyAuthAttribute();
    }

    protected override void PreInit()
    {
        _wasCalled = false;
    }

    private Task<ActionExecutedContext> Delegate()
    {
        _wasCalled = true;
        return Task.FromResult<ActionExecutedContext>(null!);
    }

    private static ActionExecutingContext CreateContext(IHeaderDictionary headers)
    {
        var request = new Mock<HttpRequest>();
        request.Setup(o => o.Headers).Returns(headers);

        var provider = TestServices.Provider.Value;
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(o => o.Request).Returns(request.Object);
        httpContext.Setup(o => o.RequestServices).Returns(provider);

        var actionContext = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());
        var controller = new AuthController(provider);

        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>()!, controller)
        {
            ActionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = typeof(AuthController).GetTypeInfo(),
                MethodInfo = typeof(AuthController).GetMethod("GetRedirectLink")!
            }
        };
    }

    private async Task InitDataAsync()
    {
        await Repository.AddCollectionAsync(new[]
        {
            new ApiClient { Name = "Test", Id = "963258740" },
            new ApiClient { Name = "Test", Id = "963258741", AllowedMethods = new List<string> { "AuthController.GetRedirectLink" } },
            new ApiClient { Name = "Test", Id = "963258743", AllowedMethods = new List<string> { "GetLink" } },
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_MissingHeader()
    {
        var context = CreateContext(new HeaderDictionary());

        await Instance.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_InvalidAuthorizationHeader()
    {
        var headers = new HeaderDictionary { { "Authorization", "Test 123" } };
        var context = CreateContext(headers);

        await Instance.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_MissingClient()
    {
        var headers = new HeaderDictionary { { "ApiKey", "12345" } };
        var context = CreateContext(headers);

        await Instance.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_NoAllowedMethods()
    {
        var headers = new HeaderDictionary { { "Authorization", "ApiKey 963258740" } };
        var context = CreateContext(headers);
        await InitDataAsync();

        await Instance.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_UnallowedMethod()
    {
        var headers = new HeaderDictionary { { "ApiKey", "963258743" } };
        var context = CreateContext(headers);
        await InitDataAsync();

        await Instance.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_Success()
    {
        var headers = new HeaderDictionary { { "ApiKey", "963258741" } };
        var context = CreateContext(headers);
        await InitDataAsync();

        await Instance.OnActionExecutionAsync(context, Delegate);
        Assert.IsTrue(_wasCalled);
    }
}
