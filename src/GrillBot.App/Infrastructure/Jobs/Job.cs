﻿using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using Quartz;
using System.Reflection;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Infrastructure.Jobs;

public abstract class Job : IJob
{
    private IServiceProvider ServiceProvider { get; }

    protected IDiscordClient DiscordClient { get; }
    protected InitManager InitManager { get; }
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

        var user = context.MergedJobDataMap.Get("User") as IUser;

        await LoggingManager.InfoAsync(JobName, $"Triggered processing at {DateTime.Now}");
        var data = new JobExecutionData
        {
            JobName = JobName,
            StartAt = DateTime.UtcNow,
            StartingUser = user is null ? null : new AuditUserInfo(user)
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

            await LoggingManager.ErrorAsync(JobName, "An error occured while job task processing.", new JobException(user, ex));
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
        if (string.IsNullOrEmpty(executionData.Result)) 
            return;

        var logRequest = new LogRequest
        {
            Type = LogType.JobCompleted,
            CreatedAt = DateTime.UtcNow,
            JobExecution = new JobExecutionRequest
            {
                Result = executionData.Result,
                EndAt = executionData.EndAt,
                JobName = executionData.JobName,
                StartAt = executionData.StartAt,
                WasError = executionData.WasError,
                StartUserId = executionData.StartingUser?.UserId
            },
            UserId = DiscordClient.CurrentUser.Id.ToString()
        };

        var auditLogServiceClient = ServiceProvider.GetRequiredService<IAuditLogServiceClient>();
        await auditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
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
