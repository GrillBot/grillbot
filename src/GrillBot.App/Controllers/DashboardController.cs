using GrillBot.App.Actions.Api.V1.Dashboard;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/dashboard")]
[ApiExplorerSettings(GroupName = "v1")]
public class DashboardController : Infrastructure.ControllerBase
{
    public DashboardController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpGet("api/{apiGroup}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DashboardInfoRow>>> GetApiDashboardAsync(string apiGroup)
        => await ProcessBridgeAsync<IAuditLogServiceClient, List<DashboardInfoRow>>(client => client.GetApiDashboardAsync(apiGroup));

    [HttpGet("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DashboardInfoRow>>> GetInteractionsDashboardAsync()
        => await ProcessBridgeAsync<IAuditLogServiceClient, List<DashboardInfoRow>>(client => client.GetInteractionsDashboardAsync());

    [HttpGet("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DashboardInfoRow>>> GetJobsDashboardAsync()
        => await ProcessBridgeAsync<IAuditLogServiceClient, List<DashboardInfoRow>>(client => client.GetJobsDashboardAsync());

    [HttpGet("todayAvgTimes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TodayAvgTimes>> GetTodayAvgTimesAsync()
        => await ProcessBridgeAsync<IAuditLogServiceClient, TodayAvgTimes>(client => client.GetTodayAvgTimes());

    [HttpGet("common")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<DashboardInfo> GetCommonInfo()
        => ProcessAction<GetCommonInfo, DashboardInfo>(action => action.Process());

    [HttpGet("services")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DashboardService>>> GetServicesListAsync()
        => await ProcessActionAsync<GetServicesList, List<DashboardService>>(action => action.ProcessAsync());

    [HttpGet("operations/active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Dictionary<string, int>> GetActiveOperations()
        => ProcessAction<GetActiveOperations, Dictionary<string, int>>(action => action.Process());

    [HttpGet("operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<CounterStats>> GetOperationStats()
        => ProcessAction<GetOperationStats, List<CounterStats>>(action => action.Process());
}
