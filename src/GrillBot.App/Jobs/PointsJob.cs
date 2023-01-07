using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.User.Points;
using Quartz;

namespace GrillBot.App.Jobs;

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
        context.Result = await PointsService.MergeOldTransactionsAsync();
    }
}
