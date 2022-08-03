using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using Quartz;

namespace GrillBot.App.Services.User.Points;

[DisallowConcurrentExecution]
public class PointsJob : Job
{
    private PointsService PointsService { get; }

    public PointsJob(LoggingService loggingService, AuditLogWriter auditLogWriter, IDiscordClient discordClient, InitManager initManager,
        PointsService pointsService) : base(loggingService, auditLogWriter, discordClient, initManager)
    {
        PointsService = pointsService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var report = new List<string>
        {
            await PointsService.ArchiveOldTransactionsAsync(),
            await PointsService.ArchiveOldSummariesAsync(),
            await PointsService.RecalculatePointsSummaryAsync()
        };

        context.Result = string.Join("\n\n", report.Where(o => o != null)).Trim();
    }
}
