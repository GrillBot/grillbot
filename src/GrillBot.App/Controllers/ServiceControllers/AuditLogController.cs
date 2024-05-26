using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

public class AuditLogController : ServiceControllerBase<IAuditLogServiceClient>
{
    public AuditLogController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpGet("dashboard/api")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetApiDashboardAsync(string apiGroup)
        => ExecuteAsync(async client => await client.GetApiDashboardAsync(apiGroup));

    [HttpGet("dashboard/interactions")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetInteractionsDashboardAsync()
        => ExecuteAsync(async client => await client.GetInteractionsDashboardAsync());

    [HttpGet("dashboard/jobs")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetJobDashboardAsync()
        => ExecuteAsync(async client => await client.GetJobsDashboardAsync());

    [HttpGet("dashboard/today-avg-times")]
    [ProducesResponseType(typeof(TodayAvgTimes), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTodayAvgTimesAsync()
        => ExecuteAsync(async client => await client.GetTodayAvgTimes());
}
