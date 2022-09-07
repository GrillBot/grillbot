using GrillBot.App.Services.User.Points;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user/points")]
public class PointsController : Controller
{
    private PointsApiService ApiService { get; }
    private ApiRequestContext Context { get; }

    public PointsController(PointsApiService apiService, ApiRequestContext context)
    {
        ApiService = apiService;
        Context = context;
    }

    /// <summary>
    /// Gets complete list of user points.
    /// </summary>
    /// <response code="200">Returns full points board.</response>
    [HttpGet("board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<UserPointsItem>>> GetPointsLeaderboardAsync()
    {
        var result = await ApiService.GetPointsBoardAsync();
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
        this.StoreParameters(parameters);

        var result = await ApiService.GetTransactionListAsync(parameters);
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
        this.StoreParameters(parameters);

        var result = await ApiService.GetSummariesAsync(parameters);
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
        this.StoreParameters(parameters);

        var result = await ApiService.GetGraphDataAsync(parameters);
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
        var result = await ApiService.ComputeUserPointsAsync(userId, false);
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
        var userId = Context.GetUserId();
        var result = await ApiService.ComputeUserPointsAsync(userId, true);
        return Ok(result);
    }
}
