using GrillBot.Common.Extensions.Services;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Graphics;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Services.SearchingService;
using GrillBot.Core.Services.UserMeasures;
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
            "rubbergod" => _serviceProvider.GetRequiredService<IRubbergodServiceClient>(),
            "graphics" => _serviceProvider.GetRequiredService<IGraphicsClient>(),
            "points" => _serviceProvider.GetRequiredService<IPointsServiceClient>(),
            "image-processing" => _serviceProvider.GetRequiredService<IImageProcessingClient>(),
            "audit-log" => _serviceProvider.GetRequiredService<IAuditLogServiceClient>(),
            "user-measures" => _serviceProvider.GetRequiredService<IUserMeasuresServiceClient>(),
            "emote" => _serviceProvider.GetRequiredService<IEmoteServiceClient>(),
            "remind" => _serviceProvider.GetRequiredService<IRemindServiceClient>(),
            "searching" => _serviceProvider.GetRequiredService<ISearchingServiceClient>(),
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
