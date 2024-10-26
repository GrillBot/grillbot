using GrillBot.App.Actions.Api.V1.Points;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user/points")]
[ApiExplorerSettings(GroupName = "v1")]
public class PointsController : Core.Infrastructure.Actions.ControllerBase
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
}
