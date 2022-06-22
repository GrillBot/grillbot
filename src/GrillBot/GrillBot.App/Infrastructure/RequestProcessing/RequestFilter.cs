using GrillBot.App.Services.User;
using GrillBot.Common.Extensions.Discord;
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
    private UserHearthbeatService UserHearthbeatService { get; }

    public RequestFilter(ApiRequest apiRequest, ApiRequestContext apiRequestContext, IDiscordClient discordClient,
        UserHearthbeatService userHearthbeatService)
    {
        ApiRequest = apiRequest;
        ApiRequestContext = apiRequestContext;
        DiscordClient = discordClient;
        UserHearthbeatService = userHearthbeatService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        await SetApiRequestContext(context);
        SetApiRequest(context);

        if (ApiRequestContext.LoggedUser != null)
            await UserHearthbeatService.UpdateHearthbeatAsync(true, ApiRequestContext);

        if (!context.ModelState.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState);
            context.Result = new BadRequestObjectResult(problemDetails);
            return;
        }

        await next();
    }

    private async Task SetApiRequestContext(ActionContext context)
    {
        if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            return;

        ApiRequestContext.LoggedUserData = context.HttpContext.User;

        var loggedUserId = ApiRequestContext.GetUserId();
        ApiRequestContext.LoggedUser = await DiscordClient.FindUserAsync(loggedUserId);
    }

    private void SetApiRequest(ActionContext context)
    {
        var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
        ApiRequest.StartAt = DateTime.Now;
        ApiRequest.TemplatePath = descriptor.AttributeRouteInfo!.Template;
        ApiRequest.Path = context.HttpContext.Request.Path.ToString();
        ApiRequest.ActionName = descriptor.MethodInfo.Name;
        ApiRequest.ControllerName = descriptor.ControllerTypeInfo.Name;
        ApiRequest.Method = context.HttpContext.Request.Method;
        ApiRequest.LoggedUserRole = ApiRequestContext.GetUserRole();
        ApiRequest.QueryParams = context.HttpContext.Request.Query.ToDictionary(o => o.Key, o => o.Value.ToString());
    }
}
