using GrillBot.Common.Infrastructure;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions;

public abstract class ApiAction
{
    protected ApiRequestContext ApiContext { get; }

    protected bool IsApiRequest { get; private set; } = true;

    protected ApiAction(ApiRequestContext apiContext)
    {
        ApiContext = apiContext;
    }

    public static void Init(Controller controller, IApiObject apiObject)
    {
        Init(controller, new[] { apiObject });
    }

    public static void Init(Controller controller, IApiObject[] apiObjects)
        => controller.StoreParameters(apiObjects);

    /// <summary>
    /// Manually update context. Use only if ApiAction is used in commands.
    /// ApiContext is configured if ApiAction was invoked from API.
    /// </summary>
    public void UpdateContext(string language, IUser loggedUser)
    {
        ApiContext.Language = TextsManager.FixLocale(language ?? "");
        ApiContext.LoggedUser = loggedUser;
        IsApiRequest = false;
    }
}
