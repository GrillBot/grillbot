using GrillBot.App.Actions.Api.V1.Dashboard;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V3.Dashboard;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Controllers;

[Route("api/dashboard")]
[ApiExplorerSettings(GroupName = "v1")]

public class DashboardController(IServiceProvider serviceProvider) : Core.Infrastructure.Actions.ControllerBase(serviceProvider)
{
    [HttpGet("api/{apiGroup}")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetApiDashboardAsync(string apiGroup)
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(
            async (client, cancellationToken) => await client.GetApiDashboardAsync(apiGroup, cancellationToken)
        );

        return ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("interactions")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetInteractionsDashboardAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(
            async (client, cancellationToken) => await client.GetInteractionsDashboardAsync(cancellationToken)
        );

        return ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetJobsDashboardAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(
            async (client, cancellationToken) => await client.GetJobsDashboardAsync(cancellationToken)
        );

        return ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("todayAvgTimes")]
    [ProducesResponseType(typeof(TodayAvgTimes), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetTodayAvgTimesAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(
            async (client, cancellationToken) => await client.GetTodayAvgTimes(cancellationToken)
        );

        return ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("userMeasures")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetNonCompliantUserMeasuresDashboardAsync()
        => ProcessAsync<GetUserMeasuresDashboard>();

    [HttpGet("common")]
    [ProducesResponseType(typeof(DashboardInfo), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetCommonInfoAsync()
        => ProcessAsync<GetCommonInfo>();

    [HttpGet("services")]
    [ProducesResponseType(typeof(List<DashboardService>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetServicesListAsync()
        => ProcessAsync<GetServicesList>();

    [HttpGet("operations/active")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetActiveOperationsAsync()
        => ProcessAsync<GetActiveOperations>();

    [HttpGet("operations")]
    [ProducesResponseType(typeof(List<CounterStats>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public Task<IActionResult> GetOperationStatsAsync()
        => ProcessAsync<GetOperationStats>();

    [JwtAuthorize("Dashboard(Admin)")]
    [HttpGet("bot-common-info")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(DashboardInfo), StatusCodes.Status200OK)]
    public Task<IActionResult> GetBotCommonInfoAsync()
        => ProcessAsync<GetBotCommonInfoAction>();

    [JwtAuthorize("Dashboard(Admin)")]
    [HttpGet("service-info/{serviceId}")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(DashboardService), StatusCodes.Status200OK)]
    public Task<IActionResult> GetServiceInfoAsync(string serviceId)
        => ProcessAsync<GetServiceInfoAction>(serviceId);

    [JwtAuthorize("Dashboard(Admin)")]
    [HttpGet("service-info/{serviceId}/detail")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(ServiceDetail), StatusCodes.Status200OK)]
    public Task<IActionResult> GetServiceDetailAsync(string serviceId)
        => ProcessAsync<GetServiceDetailAction>(serviceId);

    [JwtAuthorize("Dashboard(Admin)")]
    [HttpGet("top-heavy-operations")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(List<CounterStats>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTopHeavyOperationsAsync()
        => ProcessAsync<GetTopHeavyOperationsActions>();
}
