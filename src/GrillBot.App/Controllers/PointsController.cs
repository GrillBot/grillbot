using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Common.Services.PointsService;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Points;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user/points")]
[ApiExplorerSettings(GroupName = "v1")]
public class PointsController : Infrastructure.ControllerBase
{
    public PointsController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Gets complete list of user points.
    /// </summary>
    /// <response code="200">Returns full points board.</response>
    [HttpGet("board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserPointsItem>>> GetPointsLeaderboardAsync()
        => Ok(await ProcessActionAsync<GetPointsLeaderboard, List<UserPointsItem>>(action => action.ProcessAsync()));

    /// <summary>
    /// Get paginated list of transactions.
    /// </summary>
    /// <response code="200">Returns paginated list of transactions.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("transactions/list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<PointsTransaction>>> GetTransactionListAsync([FromBody] AdminListRequest request)
    {
        ApiAction.Init(this, request);
        return Ok(await ProcessActionAsync<GetTransactionList, PaginatedResponse<PointsTransaction>>(action => action.ProcessAsync(request)));
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
    public async Task<ActionResult<List<PointsChartItem>>> GetGraphDataAsync([FromBody] AdminListRequest parameters)
    {
        ApiAction.Init(this, parameters);

        return Ok(await ProcessActionAsync<ApiBridgeAction, List<PointsChartItem>>(
            bridge => bridge.ExecuteAsync<IPointsServiceClient, List<PointsChartItem>>(async client =>
            {
                var result = await client.GetChartDataAsync(parameters);
                result.ValidationErrors?.AggregateAndThrow();

                return result.Response!;
            })
        ));
    }

    /// <summary>
    /// Compute current points status of user.
    /// </summary>
    /// <response code="200">Returns points state of user. Grouped per guilds.</response>
    [HttpGet("{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserPointsItem>>> ComputeUserPointsAsync(ulong userId)
        => Ok(await ProcessActionAsync<ComputeUserPoints, List<UserPointsItem>>(action => action.ProcessAsync(userId)));

    /// <summary>
    /// Compute current points status of user.
    /// </summary>
    /// <response code="200">Returns points state of user. Grouped per guilds.</response>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserPointsItem>>> ComputeLoggedUserPointsAsync()
        => Ok(await ProcessActionAsync<ComputeUserPoints, List<UserPointsItem>>(action => action.ProcessAsync(null)));

    /// <summary>
    /// Creation of a service transaction by users with bonus points.
    /// </summary>
    [HttpPut("service/increment/{guildId}/{toUserId}/{amount:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ServiceIncrementPointsAsync(ulong guildId, ulong toUserId, int amount)
    {
        await ProcessActionAsync<ServiceIncrementPoints>(action => action.ProcessAsync(guildId, toUserId, amount));
        return Ok();
    }

    /// <summary>
    /// Service transfer of points between accounts.
    /// </summary>
    [HttpPut("service/transfer/{guildId}/{fromUserId}/{toUserId}/{amount:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ServiceTransferPointsAsync(ulong guildId, ulong fromUserId, ulong toUserId, int amount)
    {
        await ProcessActionAsync<ServiceTransferPoints>(action => action.ProcessAsync(guildId, fromUserId, toUserId, amount));
        return Ok();
    }
}
