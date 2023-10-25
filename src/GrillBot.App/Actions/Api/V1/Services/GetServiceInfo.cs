using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Diagnostics.Models;
using GrillBot.Core.Services.FileService;
using GrillBot.Core.Services.Graphics;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Data.Models.API.Services;

namespace GrillBot.App.Actions.Api.V1.Services;

public class GetServiceInfo : ApiAction
{
    private IGraphicsClient GraphicsClient { get; }
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private LoggingManager LoggingManager { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private IImageProcessingClient ImageProcessingClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public GetServiceInfo(ApiRequestContext apiContext, IGraphicsClient graphicsClient, IRubbergodServiceClient rubbergodServiceClient, LoggingManager loggingManager, IPointsServiceClient pointsServiceClient,
        IImageProcessingClient imageProcessingClient, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        GraphicsClient = graphicsClient;
        RubbergodServiceClient = rubbergodServiceClient;
        LoggingManager = loggingManager;
        PointsServiceClient = pointsServiceClient;
        ImageProcessingClient = imageProcessingClient;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public async Task<ServiceInfo> ProcessAsync(string id)
    {
        var client = PickClient(id);

        var info = new ServiceInfo
        {
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
            "graphics" => GraphicsClient,
            "points" => PointsServiceClient,
            "image-processing" => ImageProcessingClient,
            "audit-log" => AuditLogServiceClient,
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
                IPointsServiceClient => await PointsServiceClient.GetDiagAsync(),
                IImageProcessingClient => await ImageProcessingClient.GetDiagAsync(),
                IAuditLogServiceClient => await AuditLogServiceClient.GetDiagAsync(),
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
