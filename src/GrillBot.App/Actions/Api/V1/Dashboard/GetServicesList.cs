using AuditLog;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Executor;
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

        await AddServiceStatusAsync<Graphics.IGraphicsClient>(services, "graphics");
        await AddServiceStatusAsync<RubbergodService.IRubbergodServiceClient>(services, "rubbergod");
        await AddServiceStatusAsync<PointsService.IPointsServiceClient>(services, "points");
        await AddServiceStatusAsync<ImageProcessing.IImageProcessingClient>(services, "image-processing");
        await AddServiceStatusAsync<IAuditLogServiceClient>(services, "audit-log");
        await AddServiceStatusAsync<UserMeasures.IUserMeasuresServiceClient>(services, "user-measures");
        await AddServiceStatusAsync<Emote.IEmoteServiceClient>(services, "emote");
        await AddServiceStatusAsync<RemindService.IRemindServiceClient>(services, "remind");
        await AddServiceStatusAsync<SearchingService.ISearchingServiceClient>(services, "searching");

        if (Errors.Count == 0)
            return ApiResult.Ok(services);

        var aggregateException = new AggregateException(Errors);
        await LoggingManager.ErrorAsync("API-Dashboard", aggregateException.Message, aggregateException);

        return ApiResult.Ok(services);
    }

    private async Task AddServiceStatusAsync<TServiceClient>(List<DashboardService> services, string id) where TServiceClient : IServiceClient
    {
        try
        {
            var client = ServiceProvider.GetRequiredService<IServiceClientExecutor<TServiceClient>>();
            await client.ExecuteRequestAsync((c, ctx) => c.IsHealthyAsync(ctx.CancellationToken));

            services.Add(new DashboardService(id, true, 0));
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
        }
    }
}
