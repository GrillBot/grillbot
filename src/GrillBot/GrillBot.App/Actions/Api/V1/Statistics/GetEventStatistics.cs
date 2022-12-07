using GrillBot.Common.Managers;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetEventStatistics : ApiAction
{
    private EventLogManager EventLogManager { get; }

    public GetEventStatistics(ApiRequestContext apiContext, EventLogManager eventLogManager) : base(apiContext)
    {
        EventLogManager = eventLogManager;
    }

    public Dictionary<string, ulong> Process()
        => EventLogManager.GetStatistics();
}
