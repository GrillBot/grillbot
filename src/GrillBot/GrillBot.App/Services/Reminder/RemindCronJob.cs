using GrillBot.App.Actions.Api.V1.Reminder;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using Quartz;

namespace GrillBot.App.Services.Reminder;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class RemindCronJob : Job
{
    private FinishRemind FinishRemind { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemindCronJob(AuditLogWriter auditLogWriter, IDiscordClient discordClient, InitManager initManager, LoggingManager loggingManager, FinishRemind finishRemind,
        GrillBotDatabaseBuilder databaseBuilder) : base(auditLogWriter, discordClient, initManager, loggingManager)
    {
        DatabaseBuilder = databaseBuilder;

        FinishRemind = finishRemind;
        FinishRemind.UpdateContext("en-US", DiscordClient.CurrentUser);
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reminders = await GetIdsForProcessAsync();

        var result = new Dictionary<long, string>();
        foreach (var id in reminders)
        {
            try
            {
                await FinishRemind.ProcessAsync(id, true, false);
                result.Add(id, string.IsNullOrEmpty(FinishRemind.ErrorMessage) ? "Success" : FinishRemind.ErrorMessage);
                FinishRemind.ResetState();
            }
            catch (Exception ex)
            {
                result.Add(id, ex.Message);
                await LoggingManager.ErrorAsync(nameof(RemindCronJob), $"An error occured while processing remind #{id}", ex);
            }
        }

        if (result.Count > 0)
        {
            var resultBuilder = new StringBuilder($"Processed reminders ({reminders.Count}):").AppendLine()
                .AppendJoin("\n", result.Select(o => $"{o.Key}: {o.Value}"));

            context.Result = resultBuilder.ToString();
        }
    }

    private async Task<List<long>> GetIdsForProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Remind.GetRemindIdsForProcessAsync();
    }
}
