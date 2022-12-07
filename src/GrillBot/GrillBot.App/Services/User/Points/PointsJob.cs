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
        var report = new StringBuilder();
        
        report.AppendLine(await PointsService.MergeOldTransactionsAsync());
        report.AppendLine(await PointsService.RecalculatePointsSummaryAsync());
        report.AppendLine(await PointsService.MergeSummariesAsync());

        context.Result = report.ToString().TrimEnd('\n');
    }
}
