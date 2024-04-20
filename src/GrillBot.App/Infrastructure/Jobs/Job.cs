using GrillBot.Common.Managers;
using Quartz;
using System.Reflection;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.Services.AuditLog.Enums;
using Microsoft.Extensions.DependencyInjection;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Infrastructure.Jobs;

public abstract class Job : IJob
{
    protected static readonly string Indent = new(' ', 5);

    private IServiceProvider ServiceProvider { get; }

    protected IDiscordClient DiscordClient { get; }
    protected InitManager InitManager { get; }
    protected LoggingManager LoggingManager { get; }
    protected IRabbitMQPublisher RabbitPublisher { get; }

    private string JobName => GetType().Name;
    private bool RequireInitialization => GetType().GetCustomAttribute<DisallowUninitializedAttribute>() != null;

    protected Job(IServiceProvider serviceProvider)
    {
        DiscordClient = serviceProvider.GetRequiredService<IDiscordClient>();
        InitManager = serviceProvider.GetRequiredService<InitManager>();
        LoggingManager = serviceProvider.GetRequiredService<LoggingManager>();
        ServiceProvider = serviceProvider;
        RabbitPublisher = serviceProvider.GetRequiredService<IRabbitMQPublisher>();
    }

    protected abstract Task RunAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await CanRunAsync()) return;

        var user = context.MergedJobDataMap.Get("User") as IUser;
        await LoggingManager.InfoAsync(JobName, $"Triggered processing at {DateTime.Now}");

        var logRequest = new JobExecutionRequest
        {
            JobName = JobName,
            StartAt = DateTime.UtcNow,
            StartUserId = user?.Id.ToString()
        };

        try
        {
            await RunAsync(context);
            logRequest.Result = context.Result?.ToString() ?? "";
        }
        catch (Exception ex)
        {
            logRequest.Result = ex.ToString();
            logRequest.WasError = true;

            await LoggingManager.ErrorAsync(JobName, "An error occured while job task processing.", new JobException(user, ex));
        }
        finally
        {
            logRequest.EndAt = DateTime.UtcNow;

            await WriteToAuditLogAsync(logRequest);
            await LoggingManager.InfoAsync(JobName, $"Processing completed. Duration: {logRequest.EndAt - logRequest.StartAt}");
        }
    }

    private Task WriteToAuditLogAsync(JobExecutionRequest logRequest)
    {
        if (string.IsNullOrEmpty(logRequest.Result))
            return Task.CompletedTask;

        var userId = DiscordClient.CurrentUser.Id.ToString();
        var request = new LogRequest(LogType.JobCompleted, DateTime.UtcNow, null, userId)
        {
            JobExecution = logRequest
        };

        var payload = new CreateItemsPayload(new() { request });
        return RabbitPublisher.PublishAsync(payload, new());
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
