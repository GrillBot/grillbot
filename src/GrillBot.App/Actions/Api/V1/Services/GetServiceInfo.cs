using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Diagnostics.Models;
using GrillBot.Core.Services.Graphics;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Data.Models.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions.Api.V1.Services;

public class GetServiceInfo : ApiAction
{
    private LoggingManager LoggingManager { get; }
    private IServiceProvider ServiceProvider { get; }

    public GetServiceInfo(ApiRequestContext apiContext, LoggingManager loggingManager, IServiceProvider serviceProvider) : base(apiContext)
    {
        LoggingManager = loggingManager;
        ServiceProvider = serviceProvider;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (string)Parameters[0]!;
        var client = PickClient(id);

        var info = new ServiceInfo
        {
            Url = client.Url,
            Name = client.ServiceName
        };

        await SetDiagnosticsInfo(info, client);
        return ApiResult.Ok(info);
    }

    private IClient PickClient(string id)
    {
        return id switch
        {
            "rubbergod" => ServiceProvider.GetRequiredService<IRubbergodServiceClient>(),
            "graphics" => ServiceProvider.GetRequiredService<IGraphicsClient>(),
            "points" => ServiceProvider.GetRequiredService<IPointsServiceClient>(),
            "image-processing" => ServiceProvider.GetRequiredService<IImageProcessingClient>(),
            "audit-log" => ServiceProvider.GetRequiredService<IAuditLogServiceClient>(),
            _ => throw new NotSupportedException($"Unsupported service {id}")
        };
    }

    private async Task SetDiagnosticsInfo(ServiceInfo info, IClient client)
    {
        try
        {
            info.DiagnosticInfo = client switch
            {
                IRubbergodServiceClient rubbergodServiceClient => await rubbergodServiceClient.GetDiagAsync(),
                IGraphicsClient graphicsClient => await GetGraphicsServiceInfo(graphicsClient),
                IPointsServiceClient pointsServiceClient => await pointsServiceClient.GetDiagAsync(),
                IImageProcessingClient imageProcessingClient => await imageProcessingClient.GetDiagAsync(),
                IAuditLogServiceClient auditLogServiceClient => await auditLogServiceClient.GetDiagAsync(),
                _ => null
            };
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync("API", "An error occured while loading diagnostics info.", ex);
            info.ApiErrorMessage = ex.Message;
        }
    }

    private static async Task<DiagnosticInfo> GetGraphicsServiceInfo(IGraphicsClient graphicsClient)
    {
        var stats = await graphicsClient.GetStatisticsAsync();
        var metrics = await graphicsClient.GetMetricsAsync();

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
