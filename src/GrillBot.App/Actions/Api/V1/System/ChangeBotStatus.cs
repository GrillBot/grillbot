using GrillBot.Common.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.System;

public class ChangeBotStatus : ApiAction
{
    private InitManager InitManager { get; }

    public ChangeBotStatus(ApiRequestContext apiContext, InitManager initManager) : base(apiContext)
    {
        InitManager = initManager;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        InitManager.Set((bool)Parameters[0]!);
        return Task.FromResult(ApiResult.Ok());
    }
}
