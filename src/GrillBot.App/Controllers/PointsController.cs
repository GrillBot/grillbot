using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Core.Extensions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Services.PointsService.Models.Users;
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
    [ProducesResponseType(typeof(List<UserPointsItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPointsLeaderboardAsync()
        => await ProcessAsync<GetPointsLeaderboard>();

    /// <summary>
    /// Get paginated list of transactions.
    /// </summary>
    /// <response code="200">Returns paginated list of transactions.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("transactions/list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<PointsTransaction>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTransactionListAsync([FromBody] AdminListRequest request)
    {
        ApiAction.Init(this, request);
        return await ProcessAsync<GetTransactionList>(request);
    }

    /// <summary>
    /// Get data for graph.
    /// </summary>
    /// <response code="200">Returns data for graphs.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("graph/data")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(List<PointsChartItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGraphDataAsync([FromBody] AdminListRequest parameters)
    {
        ApiAction.Init(this, parameters);

        var executor = async (IPointsServiceClient client) =>
        {
            var result = await client.GetChartDataAsync(parameters);
            result.ValidationErrors.AggregateAndThrow();

            return result.Response;
        };

        return await ProcessAsync<ServiceBridgeAction<IPointsServiceClient>>(executor);
    }

    /// <summary>
    /// Compute current points status of user.
    /// </summary>
    /// <response code="200">Returns points state of user. Grouped per guilds.</response>
    [HttpGet("{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(List<UserPointsItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComputeUserPointsAsync(ulong userId)
        => await ProcessAsync<ComputeUserPoints>(userId);

    /// <summary>
    /// Compute current points status of user.
    /// </summary>
    /// <response code="200">Returns points state of user. Grouped per guilds.</response>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(List<UserPointsItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ComputeLoggedUserPointsAsync()
        => await ProcessAsync<ComputeUserPoints>();

    /// <summary>
    /// Creation of a service transaction by users with bonus points.
    /// </summary>
    [HttpPut("service/increment/{guildId}/{toUserId}/{amount:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ServiceIncrementPointsAsync(ulong guildId, ulong toUserId, int amount)
        => await ProcessAsync<ServiceIncrementPoints>(guildId, toUserId, amount);

    /// <summary>
    /// Service transfer of points between accounts.
    /// </summary>
    [HttpPut("service/transfer/{guildId}/{fromUserId}/{toUserId}/{amount:int}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ServiceTransferPointsAsync(ulong guildId, ulong fromUserId, ulong toUserId, int amount)
        => await ProcessAsync<ServiceTransferPoints>(guildId, fromUserId, toUserId, amount);

    /// <summary>
    /// Remove transaction.
    /// </summary>
    /// <response code="200"></response>
    [HttpDelete("{guildId}/{messageId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTransactionAsync(string guildId, string messageId, string? reactionId)
    {
        var executor = async (IPointsServiceClient client) =>
        {
            if (!string.IsNullOrEmpty(reactionId))
                await client.DeleteTransactionAsync(guildId, messageId, reactionId);
            else
                await client.DeleteTransactionAsync(guildId, messageId);
        };

        return await ProcessAsync<ServiceBridgeAction<IPointsServiceClient>>(executor);
    }

    /// <summary>
    /// Gets paginated list of users.
    /// </summary>
    /// <response code="200">Returns paginated list of users from points service.</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("users/list")]
    [ProducesResponseType(typeof(PaginatedResponse<Data.Models.API.Points.UserListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetUserListAsync(UserListRequest request)
    {
        ApiAction.Init(this, request);
        return await ProcessAsync<GetUserList>(request);
    }
}
