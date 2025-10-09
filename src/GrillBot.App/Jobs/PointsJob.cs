using GrillBot.App.Infrastructure.Jobs;
using PointsService;
using PointsService.Models;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class PointsJob(
    IPointsServiceClient _pointsServiceClient,
    IServiceProvider serviceProvider
) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var result = await MergeValidTransactionsAsync();
        if (result is not null)
        {
            const string messageTemplate = "Expired:{0}, Merged:{1}, Duration: {2}, GuildCount: {3}, UserCount: {4}, TotalPoints: {5}, DeletedDailyStats: {6}";

            var message = messageTemplate.FormatWith(result.ExpiredCount, result.MergedCount, result.Duration, result.GuildCount, result.UserCount, result.TotalPoints, result.DeletedDailyStatsCount);
            context.Result = $"MergeTransactions({message})";
        }
    }

    private async Task<MergeResult?> MergeValidTransactionsAsync()
    {
        try
        {
            return await _pointsServiceClient.MergeValidTransctionsAsync();
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }
    }
}
