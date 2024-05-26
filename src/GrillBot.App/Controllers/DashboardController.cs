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

namespace GrillBot.App.Controllers;

[Route("api/dashboard")]
[ApiExplorerSettings(GroupName = "v1")]

public class DashboardController : Core.Infrastructure.Actions.ControllerBase
{
    public DashboardController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpGet("api/{apiGroup}")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetApiDashboardAsync(string apiGroup)
    {
        var executor = new Func<IAuditLogServiceClient, Task<object>>(async (IAuditLogServiceClient client) => await client.GetApiDashboardAsync(apiGroup));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("interactions")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetInteractionsDashboardAsync()
    {
        var executor = new Func<IAuditLogServiceClient, Task<object>>(async (IAuditLogServiceClient client) => await client.GetInteractionsDashboardAsync());
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetJobsDashboardAsync()
    {
        var executor = new Func<IAuditLogServiceClient, Task<object>>(async (IAuditLogServiceClient client) => await client.GetJobsDashboardAsync());
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("todayAvgTimes")]
    [ProducesResponseType(typeof(TodayAvgTimes), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetTodayAvgTimesAsync()
    {
        var executor = new Func<IAuditLogServiceClient, Task<object>>(async (IAuditLogServiceClient client) => await client.GetTodayAvgTimes());
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("userMeasures")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetNonCompliantUserMeasuresDashboardAsync()
        => await ProcessAsync<GetUserMeasuresDashboard>();

    [HttpGet("common")]
    [ProducesResponseType(typeof(DashboardInfo), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetCommonInfoAsync()
        => await ProcessAsync<GetCommonInfo>();

    [HttpGet("services")]
    [ProducesResponseType(typeof(List<DashboardService>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetServicesListAsync()
        => await ProcessAsync<GetServicesList>();

    [HttpGet("operations/active")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetActiveOperationsAsync()
        => await ProcessAsync<GetActiveOperations>();

    [HttpGet("operations")]
    [ProducesResponseType(typeof(List<CounterStats>), StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetOperationStatsAsync()
        => await ProcessAsync<GetOperationStats>();

    [JwtAuthorize]
    [HttpGet("bot-common-info")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(DashboardInfo), StatusCodes.Status200OK)]
    public Task<IActionResult> GetBotCommonInfoAsync()
        => ProcessAsync<GetBotCommonInfoAction>();

    [JwtAuthorize]
    [HttpGet("service-info/{serviceId}")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(DashboardService), StatusCodes.Status200OK)]
    public Task<IActionResult> GetServiceInfoAsync(string serviceId)
        => ProcessAsync<GetServiceInfoAction>(serviceId);

    [JwtAuthorize]
    [HttpGet("top-heavy-operations")]
    [ApiExplorerSettings(GroupName = "v3")]
    [ProducesResponseType(typeof(List<CounterStats>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTopHeavyOperationsAsync()
        => ProcessAsync<GetTopHeavyOperationsActions>();
}
