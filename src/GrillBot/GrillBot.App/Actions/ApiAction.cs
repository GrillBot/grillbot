using System.Security.Claims;
using GrillBot.Common.Infrastructure;
using GrillBot.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Actions;

public abstract class ApiAction
{
    protected ApiRequestContext ApiContext { get; }

    protected IUser LoggedUser => ApiContext?.LoggedUser;
    protected ClaimsPrincipal LoggedUserData => ApiContext?.LoggedUserData;

    protected ApiAction(ApiRequestContext apiContext)
    {
        ApiContext = apiContext;
    }

    public void Init(Controller controller, IApiObject apiObject)
    {
        controller.StoreParameters(apiObject);
    }
}
