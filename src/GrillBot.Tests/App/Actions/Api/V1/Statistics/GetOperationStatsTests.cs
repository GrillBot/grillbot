using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetOperationStatsTests : ApiActionTest<GetOperationStats>
{
    protected override GetOperationStats CreateAction()
    {
        return new GetOperationStats(ApiRequestContext, TestServices.CounterManager.Value);
    }

    [TestMethod]
    public async Task Process()
    {
        await AddDelayAsync("Test.API");
        await AddDelayAsync("Some.API");
        await AddDelayAsync("Another.API.SubAPI");
        await AddDelayAsync("Another.API.Data");

        var result = Action.Process();

        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.Statistics.Count);
        Assert.AreNotEqual(0, result.CountChartData.Count);
        Assert.AreNotEqual(0, result.TimeChartData.Count);
    }

    private static async Task AddDelayAsync(string section)
    {
        using (TestServices.CounterManager.Value.Create(section))
        {
            await Task.Delay(10);
        }
    }
}
