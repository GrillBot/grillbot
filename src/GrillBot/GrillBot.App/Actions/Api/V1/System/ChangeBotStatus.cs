using GrillBot.Common.Managers;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.System;

public class ChangeBotStatus : ApiAction
{
    private InitManager InitManager { get; }

    public ChangeBotStatus(ApiRequestContext apiContext, InitManager initManager) : base(apiContext)
    {
        InitManager = initManager;
    }

    public void Process(bool isActive) => InitManager.Set(isActive);
}
