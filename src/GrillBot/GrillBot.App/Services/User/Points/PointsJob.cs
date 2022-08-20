using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using Quartz;

namespace GrillBot.App.Services.User.Points;

[DisallowConcurrentExecution]
public class PointsJob : Job
{
    private PointsService PointsService { get; }

    public PointsJob(AuditLogWriter auditLogWriter, IDiscordClient discordClient, InitManager initManager, PointsService pointsService, LoggingManager loggingManager) : base(auditLogWriter,
        discordClient, initManager, loggingManager)
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

        context.Result = string.Join("\n\n", report.Where(o => o != null)).Trim();
    }
}
