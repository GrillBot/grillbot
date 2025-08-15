using GrillBot.Common.Managers;
using Quartz;
using System.Reflection;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Core.Services.AuditLog.Enums;
using Microsoft.Extensions.DependencyInjection;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.App.Managers.Auth;
using GrillBot.Core.Infrastructure.Auth;

namespace GrillBot.App.Infrastructure.Jobs;

public abstract class Job(IServiceProvider serviceProvider) : IJob
{
    protected static readonly string Indent = new(' ', 5);

    protected IDiscordClient DiscordClient => ResolveService<IDiscordClient>();
    protected InitManager InitManager => ResolveService<InitManager>();
    protected LoggingManager LoggingManager => ResolveService<LoggingManager>();

    private string JobName => GetType().Name;
    private bool RequireInitialization => GetType().GetCustomAttribute<DisallowUninitializedAttribute>() != null;
    private bool RequireAuthentication => GetType().GetCustomAttribute<RequireAuthenticationAttribute>() != null;

    protected abstract Task RunAsync(IJobExecutionContext context);

    public async Task Execute(IJobExecutionContext context)
    {
        if (!await CanRunAsync()) return;

        var user = context.MergedJobDataMap.Get("User") as IUser;
        await LoggingManager.InfoAsync(JobName, $"Triggered processing at {DateTime.Now}");

        if (RequireAuthentication)
            await ProcessAuthenticationAsync();

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

        var discordClient = ResolveService<IDiscordClient>();
        var userId = discordClient.CurrentUser.Id.ToString();
        var request = new LogRequest(LogType.JobCompleted, DateTime.UtcNow, null, userId)
        {
            JobExecution = logRequest
        };

        var payload = new CreateItemsMessage(request);
        return ResolveService<IRabbitPublisher>().PublishAsync(payload);
    }

    private async Task<bool> CanRunAsync()
        => (!RequireInitialization || InitManager.Get()) && !await IsJobDisabledAsync();

    private async Task<bool> IsJobDisabledAsync()
    {
        var dataCacheManager = serviceProvider.GetRequiredService<DataCacheManager>();
        var disabledJobs = await dataCacheManager.GetValueAsync<List<string>>("DisabledJobs");

        return (disabledJobs ?? []).Contains(JobName);
    }

    protected TService ResolveService<TService>() where TService : class
        => serviceProvider.GetRequiredService<TService>();

    private async Task ProcessAuthenticationAsync()
    {
        var jwtManager = ResolveService<JwtTokenManager>();
        var jwtToken = await jwtManager.CreateTokenForUserAsync(DiscordClient.CurrentUser, "cs-CZ", "127.0.0.1");

        if (string.IsNullOrEmpty(jwtToken?.ErrorMessage) && !string.IsNullOrEmpty(jwtToken?.AccessToken))
            ResolveService<ICurrentUserProvider>().SetCustomToken(jwtToken.AccessToken);
    }
}
