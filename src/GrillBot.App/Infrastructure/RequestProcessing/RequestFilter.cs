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

public class RequestFilter(
    ApiRequestContext _apiRequestContext,
    IDiscordClient _discordClient,
    UserManager _userManager
) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        SetApiRequest(context);
        await SetApiRequestContext(context);

        if (_apiRequestContext.LoggedUser != null)
            await _userManager.SetHearthbeatAsync(true, _apiRequestContext);

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

            _apiRequestContext.Language = TextsManager.FixLocale(value.ToString());
        }

        _apiRequestContext.LogRequest.Language = _apiRequestContext.Language;
        _apiRequestContext.RemoteIp = context.HttpContext.GetRemoteIp();
    }

    private async Task SetLoggedUserAsync(ActionContext context)
    {
        _apiRequestContext.LoggedUserData = context.HttpContext.User;

        var publicType = _apiRequestContext.IsPublic() ? "Public" : "Private";
        if (!(_apiRequestContext.LoggedUserData.Identity?.IsAuthenticated ?? false))
        {
            _apiRequestContext.LogRequest.Identification = $"ApiV1({publicType}/Anonymous)";
            return;
        }

        var loggedUserId = _apiRequestContext.GetUserId();
        _apiRequestContext.LoggedUser = await _discordClient.FindUserAsync(loggedUserId);
        _apiRequestContext.LogRequest.Identification = $"ApiV1({publicType}/{_apiRequestContext.LoggedUser!.GetFullName()})";
        _apiRequestContext.LogRequest.Role = _apiRequestContext.GetRole();
    }

    private void SetApiRequest(ActionContext context)
    {
        var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
        _apiRequestContext.LogRequest.StartAt = DateTime.UtcNow;
        _apiRequestContext.LogRequest.TemplatePath = descriptor.AttributeRouteInfo!.Template!;
        _apiRequestContext.LogRequest.Path = context.HttpContext.Request.Path.ToString();
        _apiRequestContext.LogRequest.ActionName = descriptor.MethodInfo.Name;
        _apiRequestContext.LogRequest.ControllerName = descriptor.ControllerTypeInfo.Name;
        _apiRequestContext.LogRequest.Method = context.HttpContext.Request.Method;
        _apiRequestContext.LogRequest.ApiGroupName = (descriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>().LastOrDefault()?.GroupName ?? "V1").ToUpper();
        _apiRequestContext.LogRequest.Ip = context.HttpContext.Connection.RemoteIpAddress?.ToString()!;

        foreach (var item in context.HttpContext.Request.Query)
            _apiRequestContext.LogRequest.AddParameter(item.Key, item.Value.ToString());
        foreach (var (name, values) in context.HttpContext.Request.Headers)
        {
            if (name is "Authorization" or "ApiKey")
                _apiRequestContext.LogRequest.AddHeaders(name, $"<{name} header removed>");
            else
                _apiRequestContext.LogRequest.AddHeaders(name, values);
        }
    }

    private static bool IsAuthorized(ActionExecutingContext context)
    {
        var apiVersion = (context.ActionDescriptor.EndpointMetadata.OfType<ApiExplorerSettingsAttribute>().LastOrDefault()?.GroupName ?? "V1").ToUpper();
        if (apiVersion != "V3")
            return true;

        var jwtAuthorizeAttribute = context.ActionDescriptor.EndpointMetadata.OfType<JwtAuthorizeAttribute>().LastOrDefault();
        if (jwtAuthorizeAttribute is null)
            return true;

        var requiredPermissions = jwtAuthorizeAttribute.RequiredPermissions ?? [];
        var currentPermissions = context.HttpContext.User
            .FindFirst(CurrentUserProvider.GRILLBOT_PERMISSIONS_KEY)?.Value?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        return requiredPermissions.Length > 0 && requiredPermissions.Intersect(currentPermissions.Select(o => o.Trim())).Any();
    }
}
