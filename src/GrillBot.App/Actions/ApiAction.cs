using GrillBot.Common.Extensions.AuditLog;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions;

public abstract class ApiAction
{
    protected ApiRequestContext ApiContext { get; }

    protected bool IsApiRequest { get; private set; } = true;

    protected ApiAction(ApiRequestContext apiContext)
    {
        ApiContext = apiContext;
    }

    public static void Init(Controller controller, IDictionaryObject apiObject)
    {
        var apiRequestContext = controller.HttpContext.RequestServices.GetRequiredService<ApiRequestContext>();
        apiRequestContext.LogRequest.AddParameters(apiObject);
    }

    public static void Init(Controller controller, IDictionaryObject[] apiObjects)
    {
        var apiRequestContext = controller.HttpContext.RequestServices.GetRequiredService<ApiRequestContext>();
        for (var i = 0; i < apiObjects.Length; i++)
            apiRequestContext.LogRequest.AddParameters(apiObjects[i], i);
    }

    /// <summary>
    /// Manually update context. Use only if ApiAction is used in commands.
    /// ApiContext is configured if ApiAction was invoked from API.
    /// </summary>
    public void UpdateContext(string language, IUser loggedUser)
    {
        ApiContext.Language = TextsManager.FixLocale(language);
        ApiContext.LoggedUser = loggedUser;
        IsApiRequest = false;
    }
}
