using GrillBot.Common.Managers.Events;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetEventStatistics : ApiAction
{
    private EventLogManager EventLogManager { get; }

    public GetEventStatistics(ApiRequestContext apiContext, EventLogManager eventLogManager) : base(apiContext)
    {
        EventLogManager = eventLogManager;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var result = EventLogManager.GetStatistics();
        return Task.FromResult(ApiResult.Ok(result));
    }
}
