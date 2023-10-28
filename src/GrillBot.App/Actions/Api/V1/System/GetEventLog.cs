using GrillBot.Common.Managers.Events;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.System;

public class GetEventLog : ApiAction
{
    private EventLogManager EventLogManager { get; }

    public GetEventLog(ApiRequestContext apiContext, EventLogManager eventLogManager) : base(apiContext)
    {
        EventLogManager = eventLogManager;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var result = EventLogManager.GetEventLog();
        return Task.FromResult(ApiResult.Ok(result));
    }
}
