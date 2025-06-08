using GrillBot.App.Infrastructure.Auth;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.AuditLog;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GrillBot.App.Infrastructure.RequestProcessing;

public class RequestFilter : IAsyncActionFilter
{
    private ApiRequestContext ApiRequestContext { get; }
    private IDiscordClient DiscordClient { get; }
    private UserManager UserManager { get; }

    private readonly ICurrentUserProvider _currentUser;

    public RequestFilter(ApiRequestContext apiRequestContext, IDiscordClient discordClient, UserManager userManager,
        ICurrentUserProvider currentUser)
    {
        ApiRequestContext = apiRequestContext;
        DiscordClient = discordClient;
        UserManager = userManager;
        _currentUser = currentUser;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        SetApiRequest(context);
        await SetApiRequestContext(context);

        if (ApiRequestContext.LoggedUser != null)
            await UserManager.SetHearthbeatAsync(true, ApiRequestContext);

        if (!IsAuthorized(context))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

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
                throw new ValidationException(new ValidationResult("Unsupported language header value.", ["Language"]), null, value);

            ApiRequestContext.Language = TextsManager.FixLocale(value.ToString());
        }

        ApiRequestContext.LogRequest.Language = ApiRequestContext.Language;
        ApiRequestContext.RemoteIp = context.HttpContext.GetRemoteIp();
    }

    private async Task SetLoggedUserAsync(ActionContext context)
    {
        ApiRequestContext.LoggedUserData = context.HttpContext.User;

        var publicType = ApiRequestContext.IsPublic() ? "Public" : "Private";
        if (!(ApiRequestContext.LoggedUserData.Identity?.IsAuthenticated ?? false))
        {
            ApiRequestContext.LogRequest.Identification = $"ApiV1({publicType}/Anonymous)";
            return;
        }

        var loggedUserId = ApiRequestContext.GetUserId();
        ApiRequestContext.LoggedUser = await DiscordClient.FindUserAsync(loggedUserId);
        ApiRequestContext.LogRequest.Identification = $"ApiV1({publicType}/{ApiRequestContext.LoggedUser!.GetFullName()})";
        ApiRequestContext.LogRequest.Role = ApiRequestContext.GetRole();
    }

    private void SetApiRequest(ActionContext context)
    {
        var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
        ApiRequestContext.LogRequest.StartAt = DateTime.UtcNow;
        ApiRequestContext.LogRequest.TemplatePath = descriptor.AttributeRouteInfo!.Template!;
        ApiRequestContext.LogRequest.Path = context.HttpContext.Request.Path.ToString();
        ApiRequestContext.LogRequest.ActionName = descriptor.MethodInfo.Name;
        ApiRequestContext.LogRequest.ControllerName = descriptor.ControllerTypeInfo.Name;
        ApiRequestContext.LogRequest.Method = context.HttpContext.Request.Method;
        ApiRequestContext.LogRequest.ApiGroupName = (descriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>().LastOrDefault()?.GroupName ?? "V1").ToUpper();
        ApiRequestContext.LogRequest.Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString()!;

        foreach (var item in context.HttpContext.Request.Query)
            ApiRequestContext.LogRequest.AddParameter(item.Key, item.Value.ToString());
        foreach (var (name, values) in context.HttpContext.Request.Headers)
        {
            if (name is "Authorization" or "ApiKey")
                ApiRequestContext.LogRequest.AddHeaders(name, $"<{name} header removed>");
            else
                ApiRequestContext.LogRequest.AddHeaders(name, values);
        }
    }

    private bool IsAuthorized(ActionExecutingContext context)
    {
        var apiVersion = (context.ActionDescriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>().LastOrDefault()?.GroupName ?? "V1").ToUpper();
        if (apiVersion != "V3")
            return true;

        var jwtAuthorizeAttribute = context.ActionDescriptor.EndpointMetadata.OfType<JwtAuthorizeAttribute>().LastOrDefault();
        if (jwtAuthorizeAttribute is null)
            return true;

        var requiredPermissions = jwtAuthorizeAttribute.RequiredPermissions ?? Array.Empty<string>();
        return requiredPermissions.Length > 0 && requiredPermissions.Intersect(_currentUser.Permissions.Select(p => p.Trim())).Any();
    }
}
