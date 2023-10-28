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

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/dashboard")]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class DashboardController : Core.Infrastructure.Actions.ControllerBase
{
    public DashboardController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpGet("api/{apiGroup}")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiDashboardAsync(string apiGroup)
    {
        var executor = (IAuditLogServiceClient client) => client.GetApiDashboardAsync(apiGroup);
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("interactions")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInteractionsDashboardAsync()
    {
        var executor = (IAuditLogServiceClient client) => client.GetInteractionsDashboardAsync();
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("jobs")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJobsDashboardAsync()
    {
        var executor = (IAuditLogServiceClient client) => client.GetJobsDashboardAsync();
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("todayAvgTimes")]
    [ProducesResponseType(typeof(TodayAvgTimes), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayAvgTimesAsync()
    {
        var executor = (IAuditLogServiceClient client) => client.GetTodayAvgTimes();
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    [HttpGet("common")]
    [ProducesResponseType(typeof(DashboardInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommonInfoAsync()
        => await ProcessAsync<GetCommonInfo>();

    [HttpGet("services")]
    [ProducesResponseType(typeof(List<DashboardService>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServicesListAsync()
        => await ProcessAsync<GetServicesList>();

    [HttpGet("operations/active")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveOperationsAsync()
        => await ProcessAsync<GetActiveOperations>();

    [HttpGet("operations")]
    [ProducesResponseType(typeof(List<CounterStats>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperationStatsAsync()
        => await ProcessAsync<GetOperationStats>();
}
