using System.Diagnostics;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.FileService;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.ImageProcessing;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GrillBot.App.Actions.Api.V1.System;

public class GetDashboard : ApiAction
{
    private IWebHostEnvironment WebHost { get; }
    private IDiscordClient DiscordClient { get; }
    private InitManager InitManager { get; }
    private ICounterManager CounterManager { get; }
    private LoggingManager Logging { get; }
    private IGraphicsClient GraphicsClient { get; }
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private IFileServiceClient FileServiceClient { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private IImageProcessingClient ImageProcessingClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    private List<Exception> Errors { get; } = new();

    public GetDashboard(ApiRequestContext apiContext, IWebHostEnvironment webHost, IDiscordClient discordClient, InitManager initManager, ICounterManager counterManager, LoggingManager logging,
        IGraphicsClient graphicsClient, IRubbergodServiceClient rubbergodServiceClient, IFileServiceClient fileServiceClient, IPointsServiceClient pointsServiceClient,
        IImageProcessingClient imageProcessingClient, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        WebHost = webHost;
        DiscordClient = discordClient;
        InitManager = initManager;
        CounterManager = counterManager;
        Logging = logging;
        GraphicsClient = graphicsClient;
        RubbergodServiceClient = rubbergodServiceClient;
        FileServiceClient = fileServiceClient;
        PointsServiceClient = pointsServiceClient;
        ImageProcessingClient = imageProcessingClient;
        AuditLogServiceClient = auditLogServiceClient;
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
            OperationStats = ComputeOperationStats()
        };

        await ComputeDashboardInfoFromAuditLogAsync(dashboard);
        await GetServicesStatusAsync(dashboard);

        if (Errors.Count == 0)
            return dashboard;

        var aggregateException = new AggregateException(Errors);
        await Logging.ErrorAsync("API-Dashboard", aggregateException.Message, aggregateException);
        return dashboard;
    }

    private async Task ComputeDashboardInfoFromAuditLogAsync(Dashboard dashboard)
    {
        try
        {
            var dashboardInfo = await AuditLogServiceClient.GetDashboardInfoAsync();

            dashboard.TodayAvgTimes = new Dictionary<string, long>
            {
                { "InternalApi", dashboardInfo.TodayAvgTimes.PrivateApi },
                { "PublicApi", dashboardInfo.TodayAvgTimes.PublicApi },
                { "Jobs", dashboardInfo.TodayAvgTimes.Jobs },
                { "Commands", dashboardInfo.TodayAvgTimes.Interactions }
            };

            dashboard.PublicApiRequests = dashboardInfo.PublicApi.ConvertAll(o => new DashboardApiCall
            {
                Duration = o.Duration,
                Endpoint = o.Name,
                StatusCode = o.Result
            });

            dashboard.InternalApiRequests = dashboardInfo.InternalApi.ConvertAll(o => new DashboardApiCall
            {
                Duration = o.Duration,
                Endpoint = o.Name,
                StatusCode = o.Result
            });

            dashboard.Jobs = dashboardInfo.Jobs.ConvertAll(o => new DashboardJob
            {
                Duration = o.Duration,
                Name = o.Name,
                Success = o.Success
            });

            dashboard.Commands = dashboardInfo.Interactions.ConvertAll(o => new DashboardCommand
            {
                Duration = o.Duration,
                Success = o.Success,
                CommandName = o.Name
            });
        }
        catch (Exception ex)
        {
            Errors.Add(ex);

            dashboard.TodayAvgTimes = null;
            dashboard.PublicApiRequests = null;
            dashboard.InternalApiRequests = null;
            dashboard.Jobs = null;
            dashboard.Commands = null;
        }
    }

    private async Task GetServicesStatusAsync(Dashboard dashboard)
    {
        await AddServiceStatusAsync(dashboard, "graphics", GraphicsClient);
        await AddServiceStatusAsync(dashboard, "rubbergod", RubbergodServiceClient);
        await AddServiceStatusAsync(dashboard, "file", FileServiceClient);
        await AddServiceStatusAsync(dashboard, "points", PointsServiceClient);
        await AddServiceStatusAsync(dashboard, "image-processing", ImageProcessingClient);
        await AddServiceStatusAsync(dashboard, "audit-log", AuditLogServiceClient);
    }

    private async Task AddServiceStatusAsync(Dashboard dashboard, string id, IClient client)
    {
        try
        {
            dashboard.Services.Add(new DashboardService(id, client.ServiceName, await client.IsAvailableAsync()));
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
        }
    }

    private List<CounterStats> ComputeOperationStats()
    {
        var statistics = CounterManager.GetStatistics();

        return statistics
            .Select(o => new { Key = o.Section.Split('.'), Item = o })
            .GroupBy(o => o.Key.Length == 1 ? o.Key[0] : string.Join(".", o.Key.Take(o.Key.Length - 1)))
            .Select(o => new CounterStats
            {
                Count = o.Sum(x => x.Item.Count),
                Section = o.Key,
                TotalTime = o.Sum(x => x.Item.TotalTime)
            }).ToList();
    }
}
