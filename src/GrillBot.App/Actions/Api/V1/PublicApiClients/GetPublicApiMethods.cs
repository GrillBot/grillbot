using System.Reflection;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions.Api.V1.PublicApiClients;

public class GetPublicApiMethods : ApiAction
{
    public GetPublicApiMethods(ApiRequestContext apiContext) : base(apiContext)
    {
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var result = GetMethods()
            .Select(o => $"{o.DeclaringType!.Name}.{o.Name}")
            .ToList();

        return Task.FromResult(ApiResult.Ok(result));
    }

    private IEnumerable<MethodInfo> GetMethods()
    {
        var controllerType = typeof(Core.Infrastructure.Actions.ControllerBase);
        var assembly = GetType().Assembly;

        var controllers = assembly.GetTypes()
            .Where(o => !o.IsAbstract && controllerType.IsAssignableFrom(o));

        var methods = new List<MethodInfo>();
        foreach (var controller in controllers)
        {
            var controllerGroup = controller.GetCustomAttribute<ApiExplorerSettingsAttribute>()?.GroupName;

            methods.AddRange(
                controller
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(o => (o.GetCustomAttribute<ApiExplorerSettingsAttribute>()?.GroupName ?? controllerGroup)?.ToUpper() == "V2")
            );
        }

        return methods;
    }
}
