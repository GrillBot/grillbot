using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Common.Managers.Events;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetEventStatisticsTests : ApiActionTest<GetEventStatistics>
{
    protected override GetEventStatistics CreateInstance()
    {
        var eventManager = new EventLogManager();
        return new GetEventStatistics(ApiRequestContext, eventManager);
    }

    [TestMethod]
    public void Process()
    {
        var result = Instance.Process();
        Assert.AreEqual(0, result.Count);
    }
}
