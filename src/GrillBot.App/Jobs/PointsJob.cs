using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.Services.PointsService;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class PointsJob : Job
{
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsJob(IPointsServiceClient pointsServiceClient, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        PointsServiceClient = pointsServiceClient;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var result = await PointsServiceClient.MergeTransctionsAsync();
        if (result is not null)
            context.Result = $"MergeTransactions(Expired:{result.ExpiredCount}, Merged:{result.MergedCount}, {result.Duration})";
    }
}
