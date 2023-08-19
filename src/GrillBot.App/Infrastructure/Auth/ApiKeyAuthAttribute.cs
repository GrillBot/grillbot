using System.Security.Claims;
using GrillBot.Common.Models;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var apiKey = GetApiKey(context);
        if (string.IsNullOrEmpty(apiKey)) return;

        var databaseFactory = context.HttpContext.RequestServices.GetRequiredService<GrillBotDatabaseBuilder>();
        await using var repository = databaseFactory.CreateRepository();

        var apiClient = await repository.ApiClientRepository.FindClientById(apiKey);
        if (apiClient is null || apiClient.AllowedMethods.Count == 0 || apiClient.Disabled)
        {
            AsUnauthorized(context);
            return;
        }

        var descriptor = (ControllerActionDescriptor)context.ActionDescriptor;
        var method = $"{descriptor.ControllerTypeInfo.Name}.{descriptor.MethodInfo.Name}";
        if (!apiClient.AllowedMethods.Contains(method))
        {
            AsUnauthorized(context);
            return;
        }

        await IncrementStatsAsync(apiClient, repository);
        SetIdentification(context, apiClient);
        await next();
    }

    private static void AsUnauthorized(ActionExecutingContext context)
    {
        context.Result = new UnauthorizedResult();
    }

    private static string? GetApiKey(ActionExecutingContext context)
    {
        var header = context.HttpContext.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(header))
        {
            if (!header.StartsWith("ApiKey"))
            {
                AsUnauthorized(context);
                return null;
            }
        }
        else
        {
            header ??= context.HttpContext.Request.Headers["ApiKey"].FirstOrDefault();
        }

        if (!string.IsNullOrEmpty(header))
            return header.Replace("ApiKey", "").Trim();
        AsUnauthorized(context);
        return null;
    }

    private static async Task IncrementStatsAsync(ApiClient apiClient, GrillBotRepository repository)
    {
        apiClient.UseCount++;
        apiClient.LastUse = DateTime.Now;
        await repository.CommitAsync();
    }

    private static void SetIdentification(ActionContext context, ApiClient client)
    {
        var apiRequestContext = context.HttpContext.RequestServices.GetRequiredService<ApiRequestContext>();

        apiRequestContext.LogRequest.Identification = $"PublicApiV2({client.Name})";
        apiRequestContext.LoggedUserData = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, client.Name),
                new Claim(ClaimTypes.Role, "V2")
            })
        );
    }
}
