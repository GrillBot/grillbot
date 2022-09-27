using GrillBot.Common.Managers;
using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetEventStatistics : ApiAction
{
    private EventManager EventManager { get; }

    public GetEventStatistics(ApiRequestContext apiContext, EventManager eventManager) : base(apiContext)
    {
        EventManager = eventManager;
    }

    public Dictionary<string, ulong> Process()
        => EventManager.GetStatistics();
}
