using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Services.PointsService;
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
        {
            const string messageTemplate = "Expired:{0}, Merged:{1}, Duration: {2}, GuildCount: {3}, UserCount: {4}, TotalPoints: {5}";
            context.Result = $"MergeTransactions({messageTemplate.FormatWith(result.ExpiredCount, result.MergedCount, result.Duration, result.GuildCount, result.UserCount, result.TotalPoints)})";
        }
    }
}
