using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V1.Statistics;
using AuditLog;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Statistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuditLog.Models.Response.Statistics;

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
    [ProducesResponseType(typeof(AuditLogStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogStatisticsAsync()
    {
        var executor = new Func<IAuditLogServiceClient, ServiceExecutorContext, Task<object>>(async (client, ctx) => await client.GetAuditLogStatisticsAsync(ctx.CancellationToken));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Gets statistics about interactions.
    /// </summary>
    /// <response code="200">Returns statistics about interaction commannds</response>
    [HttpGet("interactions")]
    [ProducesResponseType(typeof(InteractionStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInteractionsStatusAsync()
    {
        var executor = new Func<IAuditLogServiceClient, ServiceExecutorContext, Task<object>>(async (client, ctx) => await client.GetInteractionStatisticsAsync(ctx.CancellationToken));
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
    [ProducesResponseType(typeof(ApiStatistics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiStatisticsAsync()
    {
        var executor = new Func<IAuditLogServiceClient, ServiceExecutorContext, Task<object>>(async (client, ctx) => await client.GetApiStatisticsAsync(ctx.CancellationToken));
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Get average execution times.
    /// </summary>
    [HttpGet("avg-times")]
    [ProducesResponseType(typeof(AvgExecutionTimes), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvgTimesAsync()
    {
        var executor = new Func<IAuditLogServiceClient, ServiceExecutorContext, Task<object>>(async (client, ctx) => await client.GetAvgTimesAsync(ctx.CancellationToken));
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
    [ProducesResponseType(typeof(List<Data.Models.API.Statistics.UserActionCountItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCommandStatisticsAsync()
        => await ProcessAsync<GetUserCommandStatistics>();

    /// <summary>
    /// Get statistics of api requests cross grouped with users.
    /// </summary>
    [HttpGet("api/users")]
    [ProducesResponseType(typeof(List<Data.Models.API.Statistics.UserActionCountItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserApiStatisticsAsync([Required] string criteria)
        => await ProcessAsync<GetApiUserStatistics>(criteria);
}
