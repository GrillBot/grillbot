using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Data.Models.API.Statistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
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
    {
        var result = await ProcessActionAsync<GetDatabaseStatus, DatabaseStatistics>(action => action.ProcessAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get statistics about audit logs.
    /// </summary>
    /// <response code="200">Returns statistics about audit log (by type, by date, files by count, files by size)</response>
    [HttpGet("audit-log")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditLogStatistics>> GetAuditLogStatisticsAsync()
    {
        var result = await ProcessActionAsync<GetAuditLogStatistics, AuditLogStatistics>(action => action.ProcessAsync());
        return Ok(result);
    }

    /// <summary>
    /// Gets statistics about interactions.
    /// </summary>
    /// <response code="200">Returns statistics about interaction commannds</response>
    [HttpGet("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetInteractionsStatusAsync()
    {
        var result = await ProcessActionAsync<GetCommandStatistics, List<StatisticItem>>(action => action.ProcessInteractionsAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get statistics about unverify logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per type. (Type, Count)</response>
    [HttpGet("unverify-logs/type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByOperationAsync()
    {
        var result = await ProcessActionAsync<GetUnverifyStatistics, Dictionary<string, int>>(action => action.ProcessByOperationAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get statistics about unverify logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of unverify logs statistics per date (Year-Month, Count)</response>
    [HttpGet("unverify-logs/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetUnverifyLogsStatisticsByDateAsync()
    {
        var result = await ProcessActionAsync<GetUnverifyStatistics, Dictionary<string, int>>(action => action.ProcessByDateAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get statistics about API by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of api requests per date (Year-Month, Count).</response>
    [HttpGet("api/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetApiRequestsByDateAsync()
    {
        var result = await ProcessActionAsync<GetApiStatistics, Dictionary<string, int>>(action => action.ProcessByDateAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get statistics about API by endpoint.
    /// </summary>
    /// <response code="200">Returns statistics by endpoint.</response>
    [HttpGet("api/endpoint")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetApiRequestsByEndpointAsync()
    {
        var result = await ProcessActionAsync<GetApiStatistics, List<StatisticItem>>(action => action.ProcessByEndpointAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get statistics about API by status code.
    /// </summary>
    /// <response code="200">Returns dictionary of api requests per status code (Status code, Count).</response>
    [HttpGet("api/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetApiRequestsByStatusCodeAsync()
    {
        var result = await ProcessActionAsync<GetApiStatistics, Dictionary<string, int>>(action => action.ProcessByStatusCodeAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get Discord event statistics.
    /// </summary>
    /// <response code="200">Returns dictionary of Discord event statistics (EventName, Count).</response>
    [HttpGet("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<Dictionary<string, ulong>> GetEventLogStatistics()
    {
        var result = ProcessAction<GetEventStatistics, Dictionary<string, ulong>>(action => action.Process());
        return Ok(result);
    }

    /// <summary>
    /// Get average execution times.
    /// </summary>
    [HttpGet("avg-times")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<AvgExecutionTimes>> GetAvgTimesAsync()
    {
        var result = await ProcessActionAsync<GetAvgTimes, AvgExecutionTimes>(action => action.ProcessAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get full statistics of operations.
    /// </summary>
    [HttpGet("operations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<OperationStats> GetOperationStatistics()
    {
        var result = ProcessAction<GetOperationStats, OperationStats>(action => action.Process());
        return Ok(result);
    }
}
