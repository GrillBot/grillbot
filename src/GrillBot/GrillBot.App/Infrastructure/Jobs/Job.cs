using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Quartz;
using System.Reflection;
using GrillBot.Common.Managers.Logging;

namespace GrillBot.App.Infrastructure.Jobs;

public abstract class Job : IJob
{
    private AuditLogWriter AuditLogWriter { get; }
    protected IDiscordClient DiscordClient { get; }
    private InitManager InitManager { get; }
    private LoggingManager LoggingManager { get; }

    private string JobName => GetType().Name;

    private bool RequireInitialization
        => GetType().GetCustomAttribute<DisallowUninitializedAttribute>() != null;

    private bool CanRun
        => !RequireInitialization || InitManager.Get();

    protected Job(AuditLogWriter auditLogWriter, IDiscordClient discordClient, InitManager initManager, LoggingManager loggingManager)
    {
        AuditLogWriter = auditLogWriter;
        DiscordClient = discordClient;
        InitManager = initManager;
        LoggingManager = loggingManager;
    }

    protected abstract Task RunAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        if (!CanRun) return;

        await LoggingManager.InfoAsync(JobName, $"Triggered processing at {DateTime.Now}");
        var data = new JobExecutionData
        {
            JobName = JobName,
            StartAt = DateTime.Now
        };

        try
        {
            await RunAsync(context);
            data.Result = context.Result?.ToString();
        }
        catch (Exception ex)
        {
            data.Result = ex.ToString();
            data.WasError = true;
            await LoggingManager.ErrorAsync(JobName, "An error occured while job task processing.", ex);
        }
        finally
        {
            data.MarkFinished();

            if (!string.IsNullOrEmpty(data.Result))
            {
                var item = new AuditLogDataWrapper(AuditLogItemType.JobCompleted, data, processedUser: DiscordClient.CurrentUser);
                await AuditLogWriter.StoreAsync(item);
            }

            await LoggingManager.InfoAsync(JobName, $"Processing completed. Duration: {data.EndAt - data.StartAt}");
        }
    }
}
