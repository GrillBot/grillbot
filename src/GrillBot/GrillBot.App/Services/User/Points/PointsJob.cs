using GrillBot.App.Infrastructure.Jobs;
using Quartz;

namespace GrillBot.App.Services.User.Points;

[DisallowConcurrentExecution]
public class PointsJob : Job
{
    private PointsService PointsService { get; }

    public PointsJob(PointsService pointsService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        PointsService = pointsService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var report = new List<string>
        {
            await PointsService.MergeOldTransactionsAsync(),
            await PointsService.MergeSummariesAsync(),
            await PointsService.RecalculatePointsSummaryAsync()
        };

        context.Result = string.Join("\n", report.Where(o => o != null)).Trim();
    }
}
