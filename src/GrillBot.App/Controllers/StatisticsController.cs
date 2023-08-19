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
public class StatisticsController : Infrastructure.ControllerBase
{
    public StatisticsController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get statistics about database tables.
    /// </summary>
    /// <response code="200">Returns statistics about database and cache.</response>
    [HttpGet("db")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DatabaseStatistics>> GetDbStatusAsync()
        => Ok(await ProcessActionAsync<GetDatabaseStatus, DatabaseStatistics>(action => action.ProcessAsync()));

    /// <summary>
    /// Get statistics about audit logs.
    /// </summary>
    /// <response code="200">Returns statistics about audit log (by type, by date, files by count, files by size)</response>
    [HttpGet("audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLog.AuditLogStatistics>> GetAuditLogStatisticsAsync()
        => Ok(await ProcessBridgeAsync<IAuditLogServiceClient, AuditLog.AuditLogStatistics>(client => client.GetAuditLogStatisticsAsync()));

    /// <summary>
    /// Gets statistics about interactions.
    /// </summary>
    /// <response code="200">Returns statistics about interaction commannds</response>
    [HttpGet("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditLog.StatisticItem>>> GetInteractionsStatusAsync()
        => Ok(await ProcessBridgeAsync<IAuditLogServiceClient, List<AuditLog.StatisticItem>>(client => client.GetInteractionStatisticsListAsync()));

    /// <summary>
    /// Get statistics about unverify logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per type. (Type, Count)</response>
    [HttpGet("unverify-logs/type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByOperationAsync()
        => Ok(await ProcessActionAsync<GetUnverifyStatistics, Dictionary<string, int>>(action => action.ProcessByOperationAsync()));

    /// <summary>
    /// Get statistics about unverify logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per date (Year-Month, Count)</response>
    [HttpGet("unverify-logs/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByDateAsync()
        => Ok(await ProcessActionAsync<GetUnverifyStatistics, Dictionary<string, int>>(action => action.ProcessByDateAsync()));

    /// <summary>
    /// Get statistics about API.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLog.ApiStatistics>> GetApiStatisticsAsync()
        => Ok(await ProcessBridgeAsync<IAuditLogServiceClient, AuditLog.ApiStatistics>(client => client.GetApiStatisticsAsync()));

    /// <summary>
    /// Get Discord event statistics.
    /// </summary>
    /// <response code="200">Returns dictionary of Discord event statistics (EventName, Count).</response>
    [HttpGet("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Dictionary<string, ulong>> GetEventLogStatistics()
        => Ok(ProcessAction<GetEventStatistics, Dictionary<string, ulong>>(action => action.Process()));

    /// <summary>
    /// Get average execution times.
    /// </summary>
    [HttpGet("avg-times")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLog.AvgExecutionTimes>> GetAvgTimesAsync()
        => Ok(await ProcessBridgeAsync<IAuditLogServiceClient, AuditLog.AvgExecutionTimes>(client => client.GetAvgTimesAsync()));

    /// <summary>
    /// Get full statistics of operations.
    /// </summary>
    [HttpGet("operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<OperationStats> GetOperationStatistics()
        => Ok(ProcessAction<GetOperationStats, OperationStats>(action => action.Process()));

    /// <summary>
    /// Get statistics of commands cross grouped with users. 
    /// </summary>
    [HttpGet("interactions/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserActionCountItem>>> GetUserCommandStatisticsAsync()
        => Ok(await ProcessActionAsync<GetUserCommandStatistics, List<UserActionCountItem>>(action => action.ProcessAsync()));

    /// <summary>
    /// Get statistics of api requests cross grouped with users. 
    /// </summary>
    [HttpGet("api/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserActionCountItem>>> GetUserApiStatisticsAsync([Required] string criteria)
        => Ok(await ProcessActionAsync<GetApiUserStatistics, List<UserActionCountItem>>(action => action.ProcessAsync(criteria)));
}
