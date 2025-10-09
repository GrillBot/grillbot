using AuditLog;
using GrillBot.Common.Extensions.Services;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Data.Models.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions.Api.V1.Services;

public class GetServiceInfo(
    ApiRequestContext apiContext,
    LoggingManager _loggingManager,
    IServiceProvider _serviceProvider
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var id = (string)Parameters[0]!;
        var client = PickClient(id);

        var info = new ServiceInfo
        {
            Url = client.GetServiceUrl() ?? "about:blank",
            Name = id
        };

        await SetDiagnosticsInfo(info, client);
        return ApiResult.Ok(info);
    }

    private IServiceClient PickClient(string id)
    {
        return id switch
        {
            "rubbergod" => _serviceProvider.GetRequiredService<RubbergodService.IRubbergodServiceClient>(),
            "graphics" => _serviceProvider.GetRequiredService<Graphics.IGraphicsClient>(),
            "points" => _serviceProvider.GetRequiredService<PointsService.IPointsServiceClient>(),
            "image-processing" => _serviceProvider.GetRequiredService<ImageProcessing.IImageProcessingClient>(),
            "audit-log" => _serviceProvider.GetRequiredService<IAuditLogServiceClient>(),
            "user-measures" => _serviceProvider.GetRequiredService<UserMeasures.IUserMeasuresServiceClient>(),
            "emote" => _serviceProvider.GetRequiredService<Emote.IEmoteServiceClient>(),
            "remind" => _serviceProvider.GetRequiredService<RemindService.IRemindServiceClient>(),
            "searching" => _serviceProvider.GetRequiredService<SearchingService.ISearchingServiceClient>(),
            _ => throw new NotSupportedException($"Unsupported service {id}")
        };
    }

    private async Task SetDiagnosticsInfo(ServiceInfo info, IServiceClient client)
    {
        try
        {
            info.DiagnosticInfo = await client.GetDiagnosticsAsync();
        }
        catch (Exception ex)
        {
            await _loggingManager.ErrorAsync("API", "An error occured while loading diagnostics info.", ex);
            info.ApiErrorMessage = ex.Message;
        }
    }
}
