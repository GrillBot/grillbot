using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Cooldown;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class CacheCleanerJob(
    IServiceProvider serviceProvider,
    CooldownManager _cooldownManager
) : CleanerJobBase(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reportFields = new List<string>();
        await ClearExpiredCooldownsAsync(reportFields);

        context.Result = FormatReportFromFields(reportFields);
    }

    private async Task ClearExpiredCooldownsAsync(ICollection<string> report)
    {
        var cleared = 0;
        var users = 0;

        foreach (var type in Enum.GetValues<CooldownType>())
        {
            await foreach (var user in DiscordClient.GetAllUsersAsync())
            {
                if (await _cooldownManager.RemoveCooldownIfExpired(user.Id.ToString(), type))
                    cleared++;
                users++;
            }
        }

        if (cleared > 0)
            report.Add($"ExpiredCooldowns: (Cleared: {cleared}, Users: {users})");
    }
}
