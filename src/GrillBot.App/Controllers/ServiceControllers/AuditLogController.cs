using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Request.Search;
using GrillBot.Core.Services.AuditLog.Models.Response.Detail;
using GrillBot.Core.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Core.Services.AuditLog.Models.Response.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("AuditLog(Admin)")]
public class AuditLogController(IServiceProvider serviceProvider) : ServiceControllerBase<IAuditLogServiceClient>(serviceProvider)
{
    [HttpGet("dashboard/api")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetApiDashboardAsync(string apiGroup)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetApiDashboardAsync(apiGroup, cancellationToken));

    [HttpGet("dashboard/interactions")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetInteractionsDashboardAsync()
        => ExecuteAsync(async (client, cancellationToken) => await client.GetInteractionsDashboardAsync(cancellationToken));

    [HttpGet("dashboard/jobs")]
    [ProducesResponseType(typeof(List<DashboardInfoRow>), StatusCodes.Status200OK)]
    public Task<IActionResult> GetJobDashboardAsync()
        => ExecuteAsync(async (client, cancellationToken) => await client.GetJobsDashboardAsync(cancellationToken));

    [HttpGet("dashboard/today-avg-times")]
    [ProducesResponseType(typeof(TodayAvgTimes), StatusCodes.Status200OK)]
    public Task<IActionResult> GetTodayAvgTimesAsync()
        => ExecuteAsync(async (client, cancellationToken) => await client.GetTodayAvgTimes(cancellationToken));

    [HttpPost("search")]
    [ProducesResponseType(typeof(PaginatedResponse<LogListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SearchItemsAsync(SearchRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.SearchItemsAsync(request, cancellationToken), request);

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> DeleteItemAsync(Guid id)
        => ExecuteAsync(async (client, cancellationToken) => await client.DeleteItemAsync(id, cancellationToken));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetDetailAsync(Guid id)
        => ExecuteAsync(async (client, cancellationToken) => (await client.GetDetailAsync(id, cancellationToken))!);
}
