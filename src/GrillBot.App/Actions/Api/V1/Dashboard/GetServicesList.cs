﻿using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.FileService;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.ImageProcessing;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Data.Models.API.System;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetServicesList : ApiAction
{
    private IGraphicsClient GraphicsClient { get; }
    private IRubbergodServiceClient RubbergodServiceClient { get; }
    private IFileServiceClient FileServiceClient { get; }
    private IPointsServiceClient PointsServiceClient { get; }
    private IImageProcessingClient ImageProcessingClient { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }
    private LoggingManager LoggingManager { get; }

    private List<Exception> Errors { get; } = new();

    public GetServicesList(ApiRequestContext apiContext, LoggingManager logging, IGraphicsClient graphicsClient, IRubbergodServiceClient rubbergodServiceClient, IFileServiceClient fileServiceClient,
        IPointsServiceClient pointsServiceClient, IImageProcessingClient imageProcessingClient, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        GraphicsClient = graphicsClient;
        RubbergodServiceClient = rubbergodServiceClient;
        FileServiceClient = fileServiceClient;
        ImageProcessingClient = imageProcessingClient;
        PointsServiceClient = pointsServiceClient;
        AuditLogServiceClient = auditLogServiceClient;
        LoggingManager = logging;
    }

    public async Task<List<DashboardService>> ProcessAsync()
    {
        var services = new List<DashboardService>();

        await AddServiceStatusAsync(services, "graphics", GraphicsClient);
        await AddServiceStatusAsync(services, "rubbergod", RubbergodServiceClient);
        await AddServiceStatusAsync(services, "file", FileServiceClient);
        await AddServiceStatusAsync(services, "points", PointsServiceClient);
        await AddServiceStatusAsync(services, "image-processing", ImageProcessingClient);
        await AddServiceStatusAsync(services, "audit-log", AuditLogServiceClient);

        if (Errors.Count == 0)
            return services;

        var aggregateException = new AggregateException(Errors);
        await LoggingManager.ErrorAsync("API-Dashboard", aggregateException.Message, aggregateException);
        return services;
    }

    private async Task AddServiceStatusAsync(ICollection<DashboardService> services, string id, IClient client)
    {
        try
        {
            services.Add(new DashboardService(id, client.ServiceName, await client.IsAvailableAsync()));
        }
        catch (Exception ex)
        {
            Errors.Add(ex);
        }
    }
}