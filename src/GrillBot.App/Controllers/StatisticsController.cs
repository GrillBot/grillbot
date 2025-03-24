using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Data.Models.API.Statistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuditLog = GrillBot.Core.Services.AuditLog.Models.Response.Statistics;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class StatisticsController(IServiceProvider serviceProvider) : Core.Infrastructure.Actions.ControllerBase(serviceProvider)
{

    /// <summary>
    /// Get statistics about database tables.
    /// </summary>
    /// <response code="200">Returns statistics about database and cache.</response>
    [HttpGet("db")]
    [ProducesResponseType(typeof(DatabaseStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDbStatusAsync()
        => await ProcessAsync<GetDatabaseStatus>();

    /// <summary>
    /// Get statistics about audit logs.
    /// </summary>
    /// <response code="200">Returns statistics about audit log (by type, by date, files by count, files by size)</response>
    [HttpGet("audit-log")]
    [ProducesResponseType(typeof(AuditLog.AuditLogStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogStatisticsAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(async (client, cancellationToken) => await client.GetAuditLogStatisticsAsync(cancellationToken));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Gets statistics about interactions.
    /// </summary>
    /// <response code="200">Returns statistics about interaction commannds</response>
    [HttpGet("interactions")]
    [ProducesResponseType(typeof(AuditLog.InteractionStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInteractionsStatusAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(async (client, cancellationToken) => await client.GetInteractionStatisticsAsync(cancellationToken));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Get statistics about unverify logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per type. (Type, Count)</response>
    [HttpGet("unverify-logs/type")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnverifyLogsStatisticsByOperationAsync()
        => await ProcessAsync<GetUnverifyStatistics>("ByOperation");

    /// <summary>
    /// Get statistics about unverify logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per date (Year-Month, Count)</response>
    [HttpGet("unverify-logs/date")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnverifyLogsStatisticsByDateAsync()
        => await ProcessAsync<GetUnverifyStatistics>("ByDate");

    /// <summary>
    /// Get statistics about API.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api")]
    [ProducesResponseType(typeof(AuditLog.ApiStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiStatisticsAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(async (client, cancellationToken) => await client.GetApiStatisticsAsync(cancellationToken));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Get Discord event statistics.
    /// </summary>
    /// <response code="200">Returns dictionary of Discord event statistics (EventName, Count).</response>
    [HttpGet("events")]
    [ProducesResponseType(typeof(Dictionary<string, ulong>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventLogStatisticsAsync()
        => await ProcessAsync<GetEventStatistics>();

    /// <summary>
    /// Get average execution times.
    /// </summary>
    [HttpGet("avg-times")]
    [ProducesResponseType(typeof(AuditLog.AvgExecutionTimes), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvgTimesAsync()
    {
        var executor = new Func<IAuditLogServiceClient, CancellationToken, Task<object>>(async (client, cancellationToken) => await client.GetAvgTimesAsync(cancellationToken));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Get full statistics of operations.
    /// </summary>
    [HttpGet("operations")]
    [ProducesResponseType(typeof(OperationStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOperationStatisticsAsync()
        => await ProcessAsync<GetOperationStats>();

    /// <summary>
    /// Get statistics of commands cross grouped with users.
    /// </summary>
    [HttpGet("interactions/users")]
    [ProducesResponseType(typeof(List<UserActionCountItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCommandStatisticsAsync()
        => await ProcessAsync<GetUserCommandStatistics>();

    /// <summary>
    /// Get statistics of api requests cross grouped with users.
    /// </summary>
    [HttpGet("api/users")]
    [ProducesResponseType(typeof(List<UserActionCountItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserApiStatisticsAsync([Required] string criteria)
        => await ProcessAsync<GetApiUserStatistics>(criteria);
}
