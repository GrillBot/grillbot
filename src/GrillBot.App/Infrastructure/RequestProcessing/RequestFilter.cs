using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
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
    private UserManager UserManager { get; }

    public RequestFilter(ApiRequest apiRequest, ApiRequestContext apiRequestContext, IDiscordClient discordClient, UserManager userManager)
    {
        ApiRequest = apiRequest;
        ApiRequestContext = apiRequestContext;
        DiscordClient = discordClient;
        UserManager = userManager;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        SetApiRequest(context);
        await SetApiRequestContext(context);

        if (ApiRequestContext.LoggedUser != null)
            await UserManager.SetHearthbeatAsync(true, ApiRequestContext);

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
        await SetLoggedUserAsync(context);

        if (context.HttpContext.Request.Headers.TryGetValue("Language", out var value))
        {
            if (!TextsManager.IsSupportedLocale(value.ToString()))
                throw new ValidationException(new ValidationResult("Unsupported language header value.", new[] { "Language" }), null, value);

            ApiRequestContext.Language = TextsManager.FixLocale(value.ToString());
        }

        ApiRequest.LoggedUserRole = ApiRequestContext.GetUserRole();
        ApiRequest.Language = ApiRequestContext.Language;
    }

    private async Task SetLoggedUserAsync(ActionContext context)
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
        ApiRequest.ApiGroupName = (descriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>().LastOrDefault()?.GroupName ?? "V1").ToUpper();

        foreach (var item in context.HttpContext.Request.Query)
            ApiRequest.AddParameter(item.Key, item.Value.ToString());
    }
}
