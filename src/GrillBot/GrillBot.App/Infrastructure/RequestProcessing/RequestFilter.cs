using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class RequestFilter : IAsyncActionFilter
{
    private ApiRequest ApiRequest { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private IDiscordClient DiscordClient { get; }

    public RequestFilter(ApiRequest apiRequest, ApiRequestContext apiRequestContext, IDiscordClient discordClient)
    {
        ApiRequest = apiRequest;
        ApiRequestContext = apiRequestContext;
        DiscordClient = discordClient;
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

        if (!context.ModelState.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState);
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await SetApiRequestContext(context);
        await next();
    }

    private async Task SetApiRequestContext(ActionContext context)
    {
        if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            return;

        ApiRequestContext.LoggedUserData = context.HttpContext.User;

        var loggedUserId = ApiRequestContext.LoggedUserData.GetUserId();
        ApiRequestContext.LoggedUser = await DiscordClient.FindUserAsync(loggedUserId);
    }
}
