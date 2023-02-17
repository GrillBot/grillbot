using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Statistics;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetOperationStats : ApiAction
{
    private CounterManager CounterManager { get; }

    public GetOperationStats(ApiRequestContext apiContext, CounterManager counterManager) : base(apiContext)
    {
        CounterManager = counterManager;
    }

    public OperationStats Process()
    {
        var result = new OperationStats();

        foreach (var stat in CounterManager.GetStatistics())
        {
            result.CountChartData.Add(stat.Section, stat.Count);
            result.TimeChartData.Add(stat.Section, stat.AverageTime);

            ComputeTree(result, stat);
        }

        return result;
    }

    private static void ComputeTree(OperationStats result, CounterStats stats)
    {
        var computedTree = ComputeTreeFromCounter(stats); // Compute new tree from statistics
        var currentTree = result.Statistics.Find(o => o.Section == computedTree.Section); // Find root tree to merge.
        if (currentTree == null)
        {
            // New tree. Just add to output.
            result.Statistics.Add(computedTree);
            return;
        }

        MergeTrees(computedTree, currentTree);
    }

    private static void MergeTrees(OperationStatItem computed, OperationStatItem existing)
    {
        existing.TotalTime += computed.TotalTime;
        existing.Count += computed.Count;

        foreach (var childItem in computed.ChildItems)
        {
            var childInExisting = existing.ChildItems.Find(o => o.Section == childItem.Section);
            if (childInExisting == null)
            {
                existing.ChildItems.Add(childItem);
                continue;
            }

            MergeTrees(childItem, childInExisting);
        }
    }

    private static OperationStatItem ComputeTreeFromCounter(CounterStats stats)
    {
        var fields = stats.Section.Split('.');
        var levels = fields.Select(o => new OperationStatItem { Section = o }).ToList();

        var lastLevel = levels.Last();
        lastLevel.Count = stats.Count;
        lastLevel.TotalTime = stats.TotalTime;

        return ConvertItemsToTree(levels);
    }

    private static OperationStatItem ConvertItemsToTree(IReadOnlyList<OperationStatItem> levels)
    {
        var before = levels[0];
        for (var i = 1; i < levels.Count; i++)
        {
            before.ChildItems.Add(levels[i]);
            before = levels[i];
        }

        return levels[0];
    }
}
