using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ActionFilterTest<TFilter> : ServiceTest<TFilter> where TFilter : class
{
    protected IServiceProvider Provider { get; private set; }
    protected Controller Controller { get; private set; }
    protected TFilter Filter => Service;

    protected override TFilter CreateService()
    {
        Provider = CanInitProvider() ?
            DIHelper.CreateInitializedProvider() :
            DIHelper.CreateEmptyProvider();

        Controller = CreateController(Provider);
        return CreateFilter();
    }

    protected abstract bool CanInitProvider();
    protected abstract TFilter CreateFilter();
    protected abstract Controller CreateController(IServiceProvider provider);

    private static HttpRequest CreateHttpRequest(IHeaderDictionary headers = null)
    {
        var request = new Mock<HttpRequest>();
        request.Setup(o => o.Headers).Returns(headers);
        request.Setup(o => o.Path).Returns("/api");
        request.Setup(o => o.Method).Returns("GET");
        request.Setup(o => o.Query).Returns(new QueryCollection(new Dictionary<string, StringValues>() { { "X", "Y" } }));

        return request.Object;
    }

    private HttpContext CreateHttpContext(IHeaderDictionary headers = null)
    {
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(o => o.Request).Returns(CreateHttpRequest(headers));
        httpContext.Setup(o => o.RequestServices).Returns(Provider);
        httpContext.Setup(o => o.User).Returns(new ClaimsPrincipal());

        return httpContext.Object;
    }

    private ActionContext CreateActionContext(IHeaderDictionary headers = null)
    {
        return new ActionContext(CreateHttpContext(headers), new(), new());
    }

    protected ActionExecutingContext CreateContext(string methodName, IHeaderDictionary headers = null, bool noControllerDescriptor = false)
    {
        return new ActionExecutingContext(CreateActionContext(headers), new List<IFilterMetadata>(), new Dictionary<string, object>(), Controller)
        {
            ActionDescriptor = !noControllerDescriptor ? new ControllerActionDescriptor()
            {
                ControllerTypeInfo = Controller.GetType().GetTypeInfo(),
                MethodInfo = Controller.GetType().GetMethod(methodName),
                AttributeRouteInfo = new AttributeRouteInfo() { Template = "/api" }
            } : null
        };
    }

    protected ResultExecutingContext GetContext(IActionResult result)
    {
        var actionContext = CreateActionContext();
        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, Controller);
    }

    protected ActionExecutionDelegate GetDelegate() => () => Task.FromResult<ActionExecutedContext>(null);
    protected ResultExecutionDelegate GetResultDelegate() => () => Task.FromResult<ResultExecutedContext>(null);
}
