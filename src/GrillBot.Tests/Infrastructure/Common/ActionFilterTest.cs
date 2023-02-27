using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;

namespace GrillBot.Tests.Infrastructure.Common;

[ExcludeFromCodeCoverage]
public abstract class ActionFilterTest<TFilter> : TestBase<TFilter> where TFilter : class
{
    private Controller? _controller;

    protected Controller Controller
    {
        get
        {
            _controller ??= new TestingController();
            return _controller;
        }
    }

    private static HttpRequest CreateHttpRequest(IHeaderDictionary? headers = null)
    {
        var request = new Mock<HttpRequest>();
        request.Setup(o => o.Headers).Returns(headers ?? new HeaderDictionary());
        request.Setup(o => o.Path).Returns("/api");
        request.Setup(o => o.Method).Returns("GET");
        request.Setup(o => o.Query).Returns(new QueryCollection(new Dictionary<string, StringValues> { { "X", "Y" } }));

        return request.Object;
    }

    private static HttpResponse CreateHttpResponse(int statusCode = 0)
    {
        var response = new Mock<HttpResponse>();

        if (statusCode > 0)
            response.Setup(o => o.StatusCode).Returns(statusCode);

        return response.Object;
    }

    private static HttpContext CreateHttpContext(IHeaderDictionary? headers = null, int statusCode = 0)
    {
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(o => o.Request).Returns(CreateHttpRequest(headers));
        httpContext.Setup(o => o.RequestServices).Returns(TestServices.Provider.Value);
        httpContext.Setup(o => o.User).Returns(new ClaimsPrincipal());
        httpContext.Setup(o => o.Response).Returns(CreateHttpResponse(statusCode));
        httpContext.Setup(o => o.Connection).Returns(new Mock<ConnectionInfo>().Object);

        return httpContext.Object;
    }

    private static ActionContext CreateActionContext(IHeaderDictionary? headers = null, int statusCode = 0, ModelStateDictionary? modelState = null)
    {
        modelState = modelState == null ? new ModelStateDictionary() : new ModelStateDictionary(modelState);
        return new ActionContext(CreateHttpContext(headers, statusCode), new RouteData(), new ActionDescriptor(), modelState);
    }

    protected ActionExecutingContext CreateContext(IHeaderDictionary? headers = null, bool noControllerDescriptor = false, ModelStateDictionary? modelState = null)
    {
        var actionContext = CreateActionContext(headers, modelState: modelState);

        return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>()!, Controller)
        {
            ActionDescriptor = !noControllerDescriptor
                ? new ControllerActionDescriptor
                {
                    ControllerTypeInfo = Controller.GetType().GetTypeInfo(),
                    MethodInfo = Controller.GetType().GetMethod("TestMethod")!,
                    AttributeRouteInfo = new AttributeRouteInfo { Template = "/api" }
                }
                : new ActionDescriptor()
        };
    }

    protected ResultExecutingContext GetContext(IActionResult result)
    {
        var actionContext = CreateActionContext(null, result is OkResult ? 200 : 500);
        return new ResultExecutingContext(actionContext, new List<IFilterMetadata>(), result, Controller);
    }

    protected ActionExecutionDelegate GetDelegate() => () => Task.FromResult<ActionExecutedContext>(null!);
    protected ResultExecutionDelegate GetResultDelegate() => () => Task.FromResult<ResultExecutedContext>(null!);
}
