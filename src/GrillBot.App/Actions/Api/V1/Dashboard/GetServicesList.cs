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
using GrillBot.Data.Models.API.System;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetServicesList : ApiAction
{
    private LoggingManager LoggingManager { get; }
    private IServiceProvider ServiceProvider { get; }

    private List<Exception> Errors { get; } = new();

    public GetServicesList(ApiRequestContext apiContext, LoggingManager logging, IServiceProvider serviceProvider) : base(apiContext)
    {
        LoggingManager = logging;
        ServiceProvider = serviceProvider;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var services = new List<DashboardService>();

        await AddServiceStatusAsync<IGraphicsClient>(services, "graphics");
        await AddServiceStatusAsync<IRubbergodServiceClient>(services, "rubbergod");
        await AddServiceStatusAsync<IPointsServiceClient>(services, "points");
        await AddServiceStatusAsync<IImageProcessingClient>(services, "image-processing");
        await AddServiceStatusAsync<IAuditLogServiceClient>(services, "audit-log");
        await AddServiceStatusAsync<IUserMeasuresServiceClient>(services, "user-measures");
        await AddServiceStatusAsync<IEmoteServiceClient>(services, "emote");
        await AddServiceStatusAsync<IRemindServiceClient>(services, "remind");
        await AddServiceStatusAsync<ISearchingServiceClient>(services, "searching");

        if (Errors.Count == 0)
            return ApiResult.Ok(services);

        var aggregateException = new AggregateException(Errors);
        await LoggingManager.ErrorAsync("API-Dashboard", aggregateException.Message, aggregateException);

        return ApiResult.Ok(services);
    }

    private async Task AddServiceStatusAsync<TServiceClient>(ICollection<DashboardService> services, string id) where TServiceClient : IClient
    {
        try
        {
            var client = ServiceProvider.GetRequiredService<TServiceClient>();
            services.Add(new DashboardService(id, client.ServiceName, await client.IsHealthyAsync(), 0));
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
        }
    }
}
