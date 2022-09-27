using System.Diagnostics.CodeAnalysis;
using GrillBot.Data.Models.API.Statistics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class StatisticsController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public StatisticsController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get statistics about database tables.
    /// </summary>
    /// <response code="200">Returns dictionary of database tables and records count. (TableName, Count)</response>
    [HttpGet("db")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<DatabaseStatistics>> GetDbStatusAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetDatabaseStatus>();
        var result = await action.ProcessAsync();

        return Ok(result);
    }

    /// <summary>
    /// Get statistics about audit logs by type.
    /// </summary>
    /// <response code="200">Returns dictionary of audit logs statistics per type. (Type, Count)</response>
    [HttpGet("audit-log/type")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsByTypeAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetAuditLogStatistics>();
        var result = await action.ProcessByTypeAsync();

        return Ok(result);
    }

    /// <summary>
    /// Get statistics about audit logs by date and year.
    /// </summary>
    /// <response code="200">Returns dictionary of audit logs statistics per date (Year-Month, Count)</response>
    [HttpGet("audit-log/date")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> GetAuditLogsStatisticsByDateAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetAuditLogStatistics>();
        var result = await action.ProcessByDateAsync();

        return Ok(result);
    }

    /// <summary>
    /// Gets statistics about commands.
    /// </summary>
    [HttpGet("commands")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetTextCommandStatisticsAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetCommandStatistics>();
        var result = await action.ProcessTextCommandsAsync();

        return Ok(result);
    }

    /// <summary>
    /// Gets statistics about interactions.
    /// </summary>
    [HttpGet("interactions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetInteractionsStatusAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetCommandStatistics>();
        var result = await action.ProcessInteractionsAsync();

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetUnverifyStatistics>();
        var result = await action.ProcessByOperationAsync();

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetUnverifyStatistics>();
        var result = await action.ProcessByDateAsync();

        return Ok(result);
    }

    /// <summary>
    /// Get statistics of planned background jobs.
    /// </summary>
    /// <response code="200">Returns statistics of planned jobs.</response>
    [HttpGet("jobs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StatisticItem>>> GetJobStatisticsAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetJobStatistics>();
        var result = await action.ProcessAsync();

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetApiStatistics>();
        var result = await action.ProcessByDateAsync();

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetApiStatistics>();
        var result = await action.ProcessByEndpointAsync();

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetApiStatistics>();
        var result = await action.ProcessByStatusCodeAsync();

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Statistics.GetEventStatistics>();
        var result = action.Process();
        
        return Ok(result);
    }
}
