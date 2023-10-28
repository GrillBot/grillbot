using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.User;

public class Hearthbeat : ApiAction
{
    private UserManager UserManager { get; }

    public Hearthbeat(ApiRequestContext apiContext, UserManager userManager) : base(apiContext)
    {
        UserManager = userManager;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var isActive = (bool)Parameters[0]!;
        await UserManager.SetHearthbeatAsync(isActive, ApiContext);

        return ApiResult.Ok();
    }
}
