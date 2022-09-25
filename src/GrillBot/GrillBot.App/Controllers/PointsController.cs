using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user/points")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class PointsController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public PointsController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets complete list of user points.
    /// </summary>
    /// <response code="200">Returns full points board.</response>
    [HttpGet("board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserPointsItem>>> GetPointsLeaderboardAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Points.GetPointsLeaderboard>();
        var result = await action.ProcessAsync();

        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of transactions.
    /// </summary>
    /// <response code="200">Returns paginated list of transactions.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("transactions/list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<PointsTransaction>>> GetTransactionListAsync([FromBody] GetPointTransactionsParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Points.GetTransactionList>();
        var result = await action.ProcessAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of summaries.
    /// </summary>
    /// <response code="200">Returns paginated list of summaries.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("summaries/list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<PointsSummary>>> GetSummariesAsync([FromBody] GetPointsSummaryParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Points.GetSummaries>();
        var result = await action.ProcessAsync(parameters);

        return Ok(result);
    }

    /// <summary>
    /// Get data for graph.
    /// </summary>
    /// <response code="200">Returns data for graphs.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("graph/data")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PointsSummaryBase>>> GetGraphDataAsync([FromBody] GetPointsSummaryParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Points.GetSummaryGraphData>();
        var result = await action.ProcessAsync(parameters);

        return Ok(result);
    }

    /// <summary>
    /// Compute current points status of user.
    /// </summary>
    /// <response code="200">Returns points state of user. Grouped per guilds.</response>
    [HttpGet("{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserPointsItem>>> ComputeUserPointsAsync(ulong userId)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Points.ComputeUserPoints>();
        var result = await action.ProcessAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Compute current points status of user.
    /// </summary>
    /// <response code="200">Returns points state of user. Grouped per guilds.</response>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserPointsItem>>> ComputeLoggedUserPointsAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Points.ComputeUserPoints>();
        var result = await action.ProcessAsync(null);
        return Ok(result);
    }
}
