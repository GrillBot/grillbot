using GrillBot.Common.Managers;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.System;

public class GetEventLog : ApiAction
{
    private EventLogManager EventLogManager { get; }

    public GetEventLog(ApiRequestContext apiContext, EventLogManager eventLogManager) : base(apiContext)
    {
        EventLogManager = eventLogManager;
    }

    public string[] Process() => EventLogManager.GetEventLog();
}
