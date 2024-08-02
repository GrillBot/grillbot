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
            "user-measures" => ServiceProvider.GetRequiredService<IUserMeasuresServiceClient>(),
            "emote" => ServiceProvider.GetRequiredService<IEmoteServiceClient>(),
            "remind" => ServiceProvider.GetRequiredService<IRemindServiceClient>(),
            "searching" => ServiceProvider.GetRequiredService<ISearchingServiceClient>(),
            _ => throw new NotSupportedException($"Unsupported service {id}")
        };
    }

    private async Task SetDiagnosticsInfo(ServiceInfo info, IClient client)
    {
        try
        {
            info.DiagnosticInfo = await client.GetDiagnosticAsync();
        }
        catch (Exception ex)
        {
            await LoggingManager.ErrorAsync("API", "An error occured while loading diagnostics info.", ex);
            info.ApiErrorMessage = ex.Message;
        }
    }
}
