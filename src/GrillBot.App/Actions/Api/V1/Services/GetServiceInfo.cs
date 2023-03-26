using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.FileService;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Core.Services.Diagnostics.Models;
using GrillBot.Data.Models.API.Services;

namespace GrillBot.App.Actions.Api.V1.Services;

public class GetServiceInfo : ApiAction
{
    private IGraphicsClient GraphicsClient { get; }
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private IFileServiceClient FileServiceClient { get; }
    private LoggingManager LoggingManager { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public GetServiceInfo(ApiRequestContext apiContext, IGraphicsClient graphicsClient, IRubbergodServiceClient rubbergodServiceClient, IFileServiceClient fileServiceClient,
        LoggingManager loggingManager, IPointsServiceClient pointsServiceClient) : base(apiContext)
    {
        GraphicsClient = graphicsClient;
        RubbergodServiceClient = rubbergodServiceClient;
        FileServiceClient = fileServiceClient;
        LoggingManager = loggingManager;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<ServiceInfo> ProcessAsync(string id)
    {
        var client = PickClient(id);

        var info = new ServiceInfo
        {
            Timeout = client.Timeout,
            Url = client.Url,
            Name = client.ServiceName
        };

        await SetDiagnosticsInfo(info, client);
        return info;
    }

    private IClient PickClient(string id)
    {
        return id switch
        {
            "rubbergod" => RubbergodServiceClient,
            "file" => FileServiceClient,
            "graphics" => GraphicsClient,
            "points" => PointsServiceClient,
            _ => throw new NotSupportedException($"Unsupported service {id}")
        };
    }

    private async Task SetDiagnosticsInfo(ServiceInfo info, IClient client)
    {
        try
        {
            info.DiagnosticInfo = client switch
            {
                IRubbergodServiceClient => await RubbergodServiceClient.GetDiagAsync(),
                IGraphicsClient => await GetGraphicsServiceInfo(),
                IFileServiceClient => await FileServiceClient.GetDiagAsync(),
                IPointsServiceClient => await PointsServiceClient.GetDiagAsync(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync("API", "An error occured while loading diagnostics info.", ex);
            info.ApiErrorMessage = ex.Message;
        }
    }

    private async Task<DiagnosticInfo> GetGraphicsServiceInfo()
    {
        var stats = await GraphicsClient.GetStatisticsAsync();
        var metrics = await GraphicsClient.GetMetricsAsync();

        return new DiagnosticInfo
        {
            Endpoints = stats.Endpoints,
            Uptime = metrics.Uptime,
            MeasuredFrom = stats.MeasuredFrom,
            RequestsCount = stats.RequestsCount,
            UsedMemory = metrics.UsedMemory,
            CpuTime = stats.CpuTime
        };
    }
}
