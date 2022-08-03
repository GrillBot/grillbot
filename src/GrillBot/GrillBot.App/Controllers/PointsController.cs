using GrillBot.App.Services.User.Points;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user/points")]
[OpenApiTag("Points")]
public class PointsController : Controller
{
    private PointsApiService ApiService { get; }

    public PointsController(PointsApiService apiService)
    {
        ApiService = apiService;
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
    [HttpGet("transactions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<PointsTransaction>>> GetTransactionListAsync([FromQuery] GetPointTransactionsParams parameters)
    {
        var result = await ApiService.GetTransactionListAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get paginated list of summaries.
    /// </summary>
    /// <response code="200">Returns paginated list of summaries.</response>
    /// <response code="400">Validation failed.</response>
    [HttpGet("summaries")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<PointsSummary>>> GetSummariesAsync([FromQuery] GetPointsSummaryParams parameters)
    {
        var result = await ApiService.GetSummariesAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get data for graph.
    /// </summary>
    /// <response code="200">Returns data for graphs.</response>
    /// <response code="400">Validation failed.</response>
    [HttpGet("graph")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<PointsSummaryBase>>> GetGraphDataAsync([FromQuery] GetPointsSummaryParams parameters)
    {
        var result = await ApiService.GetGraphDataAsync(parameters);
        return Ok(result);
    }
}
