using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Quartz;

namespace GrillBot.App.Infrastructure;

public abstract class Job : IJob
{
    protected LoggingService LoggingService { get; }
    protected AuditLogService AuditLogService { get; }
    protected IDiscordClient DiscordClient { get; }

    private string JobName => GetType().Name;

    protected Job(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient)
    {
        LoggingService = loggingService;
        AuditLogService = auditLogService;
        DiscordClient = discordClient;
    }

    public abstract Task RunAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        await LoggingService.InfoAsync(JobName, $"Triggered processing at {DateTime.Now}");

        var data = new JobExecutionData()
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
            await LoggingService.ErrorAsync(JobName, "An error occured while job task processing.", ex);
        }
        finally
        {
            data.MarkFinished();
            var item = new AuditLogDataWrapper(AuditLogItemType.JobCompleted, data, processedUser: DiscordClient.CurrentUser);

            await AuditLogService.StoreItemAsync(item);
            await LoggingService.InfoAsync(JobName, $"Processing completed. Duration: {data.EndAt - data.StartAt}");
        }
    }
}
