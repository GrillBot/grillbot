using GrillBot.Common.Extensions.Services;
using GrillBot.Common.Managers.Logging;
using GrillBot.Common.Models;
using GrillBot.Core.Exceptions;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.PointsService;
using GrillBot.Data.Models.API.System;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System.Reflection;

namespace GrillBot.App.Actions.Api.V3.Dashboard;

public class GetServiceDetailAction(
    ApiRequestContext apiContext,
    IServiceProvider _serviceProvider,
    LoggingManager _logging,
    IConfiguration _configuration
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var serviceId = GetParameter<string>(0);
        var client = GetClient(serviceId)
            ?? throw new NotFoundException($"Unable to find service with ID {serviceId}");

        var detail = new ServiceDetail
        {
            Name = serviceId,
            Url = client.GetServiceUrl() ?? "http://localhost"
        };

        await SetDiagnosticsDataAsync(detail, client);
        await SetAdditionalDataAsync(detail, client);

        return ApiResult.Ok(detail);
    }

    private IServiceClient? GetClient(string serviceId)
    {
        return typeof(IServiceClient).Assembly.GetTypes()
            .Where(o => o.IsInterface && o.GetInterface(nameof(IServiceClient)) is not null)
            .Select(_serviceProvider.GetService)
            .OfType<IServiceClient>()
            .FirstOrDefault(client =>
            {
                return client.GetType().GetInterfaces()
                    .Where(@interface => @interface.Name != nameof(IServiceClient))
                    .Select(@interface => typeof(SettingsFor<>).MakeGenericType(@interface))
                    .Select(@interface => (_serviceProvider.GetService(@interface) as ISettingsFor)?.Settings)
                    .Select(settings => settings?.HttpRequestMessageOptions?.TryGetValue("ServiceName", out var name) == true ? name?.ToString() : null)
                    .Any(name => !string.IsNullOrEmpty(name) && name == serviceId);
            });
    }

    private async Task SetDiagnosticsDataAsync(ServiceDetail detail, IServiceClient client)
    {
        try
        {
            var diagnostics = await client.GetDiagnosticsAsync();

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

    private async Task SetAdditionalDataAsync(ServiceDetail detail, IServiceClient client)
    {
        object? additionalInfo = null;

        if (client is IAuditLogServiceClient auditLog)
        {
            var executor = new ServiceClientExecutor<IAuditLogServiceClient>(_configuration, auditLog);
            additionalInfo = await executor.ExecuteRequestAsync((c, cancellationToken) => c.GetStatusInfoAsync(cancellationToken));
        }
        else if (client is IPointsServiceClient pointsClient)
        {
            var executor = new ServiceClientExecutor<IPointsServiceClient>(_configuration, pointsClient);
            additionalInfo = await executor.ExecuteRequestAsync((c, cancellationToken) => c.GetStatusInfoAsync(cancellationToken));
        }

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
