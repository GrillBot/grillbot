using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Quartz;
using System.Reflection;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Jobs;

public abstract class Job : IJob
{
    private IServiceProvider ServiceProvider { get; }

    protected IDiscordClient DiscordClient { get; }
    private InitManager InitManager { get; }
    protected LoggingManager LoggingManager { get; }

    private string JobName => GetType().Name;

    private bool RequireInitialization
        => GetType().GetCustomAttribute<DisallowUninitializedAttribute>() != null;

    protected Job(IServiceProvider serviceProvider)
    {
        DiscordClient = serviceProvider.GetRequiredService<IDiscordClient>();
        InitManager = serviceProvider.GetRequiredService<InitManager>();
        LoggingManager = serviceProvider.GetRequiredService<LoggingManager>();
        ServiceProvider = serviceProvider;
    }

    protected abstract Task RunAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await CanRunAsync()) return;

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

            await WriteToAuditLogAsync(data);
            await LoggingManager.InfoAsync(JobName, $"Processing completed. Duration: {data.EndAt - data.StartAt}");
        }
    }

    private async Task WriteToAuditLogAsync(JobExecutionData executionData)
    {
        if (string.IsNullOrEmpty(executionData.Result)) return;

        var auditLogWriter = ServiceProvider.GetRequiredService<AuditLogWriter>();
        var logItem = new AuditLogDataWrapper(AuditLogItemType.JobCompleted, executionData, processedUser: DiscordClient.CurrentUser);
        await auditLogWriter.StoreAsync(logItem);
    }

    private async Task<bool> CanRunAsync()
        => (!RequireInitialization || InitManager.Get()) && !await IsJobDisabledAsync();

    private async Task<bool> IsJobDisabledAsync()
    {
        var dataCacheManager = ServiceProvider.GetRequiredService<DataCacheManager>();
        var data = await dataCacheManager.GetValueAsync("DisabledJobs");
        if (string.IsNullOrEmpty(data)) data = "[]";

        var disabledJobs = JsonConvert.DeserializeObject<List<string>>(data);
        return disabledJobs!.Contains(JobName);
    }
}
