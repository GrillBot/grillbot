using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class RequestFilter : IAsyncActionFilter
{
    private ApiRequest ApiRequest { get; }

    public RequestFilter(ApiRequest apiRequest)
    {
        ApiRequest = apiRequest;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;

        ApiRequest.StartAt = DateTime.Now;
        ApiRequest.TemplatePath = context.ActionDescriptor.AttributeRouteInfo.Template;
        ApiRequest.Path = context.HttpContext.Request.Path.ToString();
        ApiRequest.ActionName = descriptor.MethodInfo.Name;
        ApiRequest.ControllerName = descriptor.ControllerTypeInfo.Name;
        ApiRequest.Method = context.HttpContext.Request.Method;
        ApiRequest.LoggedUserRole = context.HttpContext.User.GetUserRole();
        ApiRequest.QueryParams = context.HttpContext.Request.Query.ToDictionary(o => o.Key, o => o.Value.ToString());

        await next();
    }
}
