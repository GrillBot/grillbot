using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyConfigKey = "Auth:ApiKeys";

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var header = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(header))
        {
            if (!header.StartsWith("ApiKey"))
                return AsUnauthorized(context);
        }
        else
        {
            header ??= context.HttpContext.Request.Headers["ApiKey"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(header))
            return AsUnauthorized(context);

        header = header.Replace("ApiKey", "").Trim();

        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>()
            .GetSection($"{ApiKeyConfigKey}:{header}");

        if (!configuration.Exists())
            return AsUnauthorized(context);

        var allowedMethods = configuration.GetSection("AuthorizedMethods")
            .AsEnumerable()
            .Select(o => o.Value)
            .Where(o => !string.IsNullOrEmpty(o))
            .ToList();

        if (allowedMethods.Count == 0)
            return AsUnauthorized(context);

        if (allowedMethods.Count == 1 && allowedMethods[0] == "*")
            return next();

        if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return AsUnauthorized(context);

        var method = $"{descriptor.ControllerTypeInfo.Name}.{descriptor.MethodInfo.Name}";
        if (!allowedMethods.Contains(method))
            return AsUnauthorized(context);

        return next();
    }

    private static Task AsUnauthorized(ActionExecutingContext context)
    {
        context.Result = new UnauthorizedResult();
        return Task.CompletedTask;
    }
}
