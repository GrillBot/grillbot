using Castle.Components.DictionaryAdapter;
using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Infrastructure.Auth;

[TestClass]
public class ApiKeyAuthAttributeTests
{
    private bool _wasCalled;

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

    private static ActionExecutingContext CreateContext(IHeaderDictionary headers, bool noDescriptor = false)
    {
        var request = new Mock<HttpRequest>();
        request.Setup(o => o.Headers).Returns(headers);

        var serviceProvider = DIHelper.CreateInitializedProvider();
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(o => o.Request).Returns(request.Object);
        httpContext.Setup(o => o.RequestServices).Returns(serviceProvider);

        var actionContext = new ActionContext(httpContext.Object, new(), new());
        var controller = new AuthController(null);

        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), controller)
        {
            ActionDescriptor = !noDescriptor ? new ControllerActionDescriptor()
            {
                ControllerTypeInfo = typeof(AuthController).GetTypeInfo(),
                MethodInfo = typeof(AuthController).GetMethod("GetRedirectLink")
            } : null
        };
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
        var headers = new HeaderDictionary
        {
            { "Authorization", "Test 123" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_MissingConfiguration()
    {
        var headers = new HeaderDictionary()
        {
            { "ApiKey", "12345" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_NoAllowedMethods()
    {
        var headers = new HeaderDictionary()
        {
            { "ApiKey", "963258740" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_AllowAll()
    {
        var headers = new HeaderDictionary()
        {
            { "ApiKey", "963258742" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsTrue(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_NoDescriptor()
    {
        var headers = new HeaderDictionary()
        {
            { "ApiKey", "963258743" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers, true);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_UnallowedMethod()
    {
        var headers = new HeaderDictionary()
        {
            { "ApiKey", "963258743" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
        Assert.IsFalse(_wasCalled);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_Success()
    {
        var headers = new HeaderDictionary()
        {
            { "ApiKey", "963258741" }
        };

        var attribute = new ApiKeyAuthAttribute();
        var context = CreateContext(headers);

        await attribute.OnActionExecutionAsync(context, Delegate);
        Assert.IsTrue(_wasCalled);
    }
}
