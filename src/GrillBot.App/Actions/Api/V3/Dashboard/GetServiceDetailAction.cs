using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.PointsService;
using GrillBot.Data.Models.API.System;
using System.Reflection;

namespace GrillBot.App.Actions.Api.V3.Dashboard;

public class GetServiceDetailAction : ApiAction
{
    private readonly IServiceProvider _serviceProvider;
    private readonly LoggingManager _logging;

    public GetServiceDetailAction(ApiRequestContext apiContext, IServiceProvider serviceProvider, LoggingManager logging) : base(apiContext)
    {
        _serviceProvider = serviceProvider;
        _logging = logging;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var serviceId = GetParameter<string>(0);
        var client = GetClient(serviceId)
            ?? throw new NotFoundException($"Unable to find service with ID {serviceId}");

        var detail = new ServiceDetail
        {
            Name = client.ServiceName,
            Url = client.Url
        };

        await SetDiagnosticsDataAsync(detail, client);
        await SetAdditionalDataAsync(detail, client);

        return ApiResult.Ok(detail);
    }

    private IClient? GetClient(string serviceId)
    {
        return typeof(IClient).Assembly.GetTypes()
            .Where(o => o.IsInterface && o.GetInterface(nameof(IClient)) is not null)
            .Select(_serviceProvider.GetService)
            .OfType<IClient>()
            .FirstOrDefault(o => o.ServiceName == serviceId);
    }

    private async Task SetDiagnosticsDataAsync(ServiceDetail detail, IClient client)
    {
        try
        {
            var diagnostics = await client.GetDiagnosticAsync();

            detail.UsedMemory = diagnostics.UsedMemory;
            detail.Uptime = diagnostics.Uptime;
            detail.RequestsCount = diagnostics.RequestsCount;
            detail.CpuTime = diagnostics.CpuTime;
            detail.MeasuredFrom = diagnostics.MeasuredFrom;
            detail.Endpoints = diagnostics.Endpoints;
            detail.DatabaseStatistics = diagnostics.DatabaseStatistics;
            detail.Operations = diagnostics.Operations;
        }
        catch (Exception ex)
        {
            await _logging.ErrorAsync("API", "An error occured while loading diagnostics info.", ex);
            detail.ApiErrorMessage = ex.Message;
        }
    }

    private static async Task SetAdditionalDataAsync(ServiceDetail detail, IClient client)
    {
        object? additionalInfo = null;

        if (client is IAuditLogServiceClient auditLog)
            additionalInfo = await auditLog.GetStatusInfoAsync();
        else if (client is IPointsServiceClient points)
            additionalInfo = await points.GetStatusInfoAsync();

        if (additionalInfo is null)
            return;

        detail.AdditionalData = additionalInfo
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty)
            .Select(o => new KeyValuePair<string, string?>(o.Name, o.GetValue(additionalInfo)?.ToString()))
            .Where(o => !string.IsNullOrEmpty(o.Value))
            .ToDictionary(o => o.Key, o => o.Value!);
    }
}
