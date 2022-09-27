using GrillBot.Common.Managers;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.System;

public class GetEventLog : ApiAction
{
    private EventManager EventManager { get; }

    public GetEventLog(ApiRequestContext apiContext, EventManager eventManager) : base(apiContext)
    {
        EventManager = eventManager;
    }

    public string[] Process() => EventManager.GetEventLog();
}
