using System.Diagnostics;
using GrillBot.App.Controllers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Common.Services.Graphics;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.System;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App.Actions.Api.V1.System;

public class GetDashboard : ApiAction
{
    private const int LogRowsSize = 10;

    private IWebHostEnvironment WebHost { get; }
    private IDiscordClient DiscordClient { get; }
    private InitManager InitManager { get; }
    private CounterManager CounterManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private LoggingManager Logging { get; }
    private IGraphicsClient GraphicsClient { get; }

    private List<Exception> Errors { get; } = new();

    public GetDashboard(ApiRequestContext apiContext, IWebHostEnvironment webHost, IDiscordClient discordClient, InitManager initManager, CounterManager counterManager,
        GrillBotDatabaseBuilder databaseBuilder, LoggingManager logging, IGraphicsClient graphicsClient) : base(apiContext)
    {
        WebHost = webHost;
        DiscordClient = discordClient;
        InitManager = initManager;
        CounterManager = counterManager;
        DatabaseBuilder = databaseBuilder;
        Logging = logging;
        GraphicsClient = graphicsClient;
    }

    public async Task<Dashboard> ProcessAsync()
    {
        var process = Process.GetCurrentProcess();

        var dashboard = new Dashboard
        {
            IsDevelopment = WebHost.IsDevelopment(),
            StartAt = process.StartTime,
            Uptime = Convert.ToInt64((DateTime.Now - process.StartTime).TotalMilliseconds),
            CpuTime = Convert.ToInt64(process.TotalProcessorTime.TotalMilliseconds),
            ConnectionState = DiscordClient.ConnectionState,
            UsedMemory = process.WorkingSet64,
            IsActive = InitManager.Get(),
            CurrentDateTime = DateTime.Now,
            ActiveOperations = CounterManager.GetActiveCounters(),
            OperationStats = CounterManager.GetStatistics()
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        await ComputeTodayAvgTimesAsync(repository, dashboard);
        await ComputeApiLogAsync(repository, dashboard);
        await ComputeJobsLogAsync(repository, dashboard);
        await ComputeCommandsLogAsync(repository, dashboard);
        await GetServicesStatusAsync(dashboard);

        if (Errors.Count == 0) return dashboard;

        var aggregateException = new AggregateException(Errors);
        await Logging.ErrorAsync("API-Dashboard", aggregateException.Message, aggregateException);
        return dashboard;
    }

    private async Task ComputeTodayAvgTimesAsync(GrillBotRepository repository, Dashboard dashboard)
    {
        try
        {
            var parameters = new AuditLogListParams
            {
                Sort = null,
                Types = new List<AuditLogItemType> { AuditLogItemType.Api, AuditLogItemType.InteractionCommand, AuditLogItemType.JobCompleted },
                CreatedFrom = DateTime.Now.Date,
                CreatedTo = DateTime.Now.Date.Add(new TimeSpan(23, 59, 59)),
            };

            var auditLogs = await repository.AuditLog.GetSimpleDataAsync(parameters);
            var groupedData = auditLogs.GroupBy(o => o.Type).ToDictionary(o => o.Key, o => o.ToList());

            dashboard.TodayAvgTimes = new Dictionary<string, long>
            {
                { "InternalApi", 0L },
                { "PublicApi", 0L },
                { "Jobs", 0L },
                { "Commands", 0L }
            };

            if (groupedData.TryGetValue(AuditLogItemType.Api, out var logs))
            {
                var logItems = logs.ConvertAll(o => JsonConvert.DeserializeObject<ApiRequest>(o.Data, AuditLogWriteManager.SerializerSettings)!);
                dashboard.TodayAvgTimes["InternalApi"] = ComputeApiAvgTime(logItems, false);
                dashboard.TodayAvgTimes["PublicApi"] = ComputeApiAvgTime(logItems, true);
            }

            if (groupedData.TryGetValue(AuditLogItemType.JobCompleted, out logs))
            {
                var logItems = logs.Select(o => JsonConvert.DeserializeObject<JobExecutionData>(o.Data, AuditLogWriteManager.SerializerSettings)!);
                dashboard.TodayAvgTimes["Jobs"] = (long)logItems.Average(o => Convert.ToInt64(o.Duration()));
            }

            if (groupedData.TryGetValue(AuditLogItemType.InteractionCommand, out logs))
            {
                var logItems = logs.Select(o => JsonConvert.DeserializeObject<InteractionCommandExecuted>(o.Data, AuditLogWriteManager.SerializerSettings)!);
                dashboard.TodayAvgTimes["Commands"] = (long)logItems.Average(o => o.Duration);
            }
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
            dashboard.TodayAvgTimes = null;
        }
    }

    private static long ComputeApiAvgTime(List<ApiRequest> items, bool isPublic)
    {
        bool CanProcess(ApiRequest request)
        {
            if (request.IsCorrupted()) return false;
            return (isPublic && (request.ApiGroupName ?? "V1") == "V2") || (!isPublic && (request.ApiGroupName ?? "V1") == "V1");
        }

        items = items.FindAll(CanProcess);
        return items.Count == 0
            ? 0
            : (long)items.Average(o => Convert.ToInt64((o.EndAt - o.StartAt).TotalMilliseconds));
    }

    private async Task ComputeApiLogAsync(GrillBotRepository repository, Dashboard dashboard)
    {
        try
        {
            dashboard.InternalApiRequests = new List<DashboardApiCall>();
            dashboard.PublicApiRequests = new List<DashboardApiCall>();

            var parameters = new AuditLogListParams
            {
                Sort = { OrderBy = "CreatedAt", Descending = true },
                Types = new List<AuditLogItemType> { AuditLogItemType.Api },
                CreatedFrom = DateTime.Now.Date.AddMonths(-1)
            };

            var auditLogs = await repository.AuditLog.GetSimpleDataAsync(parameters);
            foreach (var logItem in auditLogs)
            {
                if (dashboard.PublicApiRequests.Count == LogRowsSize && dashboard.InternalApiRequests.Count == LogRowsSize) break;

                var request = JsonConvert.DeserializeObject<ApiRequest>(logItem.Data)!;
                if (request.IsCorrupted()) continue;
                if (request is { ControllerName: nameof(SystemController), ActionName: nameof(SystemController.GetDashboardAsync) }) continue;

                var row = new DashboardApiCall
                {
                    Duration = Convert.ToInt64((request.EndAt - request.StartAt).TotalMilliseconds),
                    Endpoint = $"{request.Method} {request.TemplatePath}",
                    StatusCode = request.StatusCode
                };

                if ((request.ApiGroupName ?? "V1") == "V2")
                {
                    if (dashboard.PublicApiRequests.Count < LogRowsSize)
                        dashboard.PublicApiRequests.Add(row);
                }
                else
                {
                    if (dashboard.InternalApiRequests.Count < LogRowsSize)
                        dashboard.InternalApiRequests.Add(row);
                }
            }
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
            dashboard.InternalApiRequests = null;
            dashboard.PublicApiRequests = null;
        }
    }

    private async Task ComputeJobsLogAsync(GrillBotRepository repository, Dashboard dashboard)
    {
        try
        {
            var parameters = new AuditLogListParams
            {
                Sort = { OrderBy = "CreatedAt", Descending = true },
                Types = new List<AuditLogItemType> { AuditLogItemType.JobCompleted }
            };

            var auditLogs = await repository.AuditLog.GetSimpleDataAsync(parameters, LogRowsSize);
            dashboard.Jobs = auditLogs
                .Select(o => JsonConvert.DeserializeObject<JobExecutionData>(o.Data, AuditLogWriteManager.SerializerSettings)!)
                .Select(o => new DashboardJob
                {
                    Duration = o.Duration(),
                    Name = o.JobName,
                    Success = !o.WasError
                }).ToList();
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
            dashboard.Jobs = null;
        }
    }

    private async Task ComputeCommandsLogAsync(GrillBotRepository repository, Dashboard dashboard)
    {
        try
        {
            var parameters = new AuditLogListParams
            {
                Sort = { OrderBy = "CreatedAt", Descending = true },
                Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand }
            };

            var auditLogs = await repository.AuditLog.GetSimpleDataAsync(parameters, LogRowsSize);
            dashboard.Commands = auditLogs
                .Select(logItem => JsonConvert.DeserializeObject<InteractionCommandExecuted>(logItem.Data, AuditLogWriteManager.SerializerSettings)!)
                .Select(o => new DashboardCommand
                {
                    Duration = o.Duration,
                    Success = o.IsSuccess,
                    CommandName = o.FullName
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
            dashboard.Commands = null;
        }
    }

    private async Task GetServicesStatusAsync(Dashboard dashboard)
    {
        try
        {
            dashboard.Services = new DashboardServices
            {
                GraphicsAvailable = await GraphicsClient.IsAvailableAsync()
            };
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
            dashboard.Services = null;
        }
    }
}
