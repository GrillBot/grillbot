using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Quartz;
using System.Reflection;

namespace GrillBot.App.Infrastructure.Jobs;

public abstract class Job : IJob
{
    protected LoggingService LoggingService { get; }
    private AuditLogWriter AuditLogWriter { get; }
    protected IDiscordClient DiscordClient { get; }
    private InitManager InitManager { get; }

    protected string JobName => GetType().Name;

    private bool RequireInitialization
        => GetType().GetCustomAttribute<DisallowUninitializedAttribute>() != null;

    private bool CanRun
        => !RequireInitialization || InitManager.Get();

    protected Job(LoggingService loggingService, AuditLogWriter auditLogWriter, IDiscordClient discordClient,
        InitManager initManager)
    {
        LoggingService = loggingService;
        AuditLogWriter = auditLogWriter;
        DiscordClient = discordClient;
        InitManager = initManager;
    }

    protected abstract Task RunAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        if (!CanRun) return;

        await LoggingService.InfoAsync(JobName, $"Triggered processing at {DateTime.Now}");
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
            await LoggingService.ErrorAsync(JobName, "An error occured while job task processing.", ex);
        }
        finally
        {
            data.MarkFinished();

            if (!string.IsNullOrEmpty(data.Result))
            {
                var item = new AuditLogDataWrapper(AuditLogItemType.JobCompleted, data, processedUser: DiscordClient.CurrentUser);
                await AuditLogWriter.StoreAsync(item);
            }

            await LoggingService.InfoAsync(JobName, $"Processing completed. Duration: {data.EndAt - data.StartAt}");
        }
    }
}
