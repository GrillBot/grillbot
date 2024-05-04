using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.User;
using GrillBot.App.Actions.Api.V2;
using GrillBot.App.Actions.Api.V2.User;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Services.RubbergodService.Models.Karma;
using GrillBot.Core.Models.Pagination;
using GrillBot.App.Actions.Api;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user")]
[ApiExplorerSettings(GroupName = "v1")]
public class UsersController : Core.Infrastructure.Actions.ControllerBase
{
    public UsersController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get paginated list of users.
    /// </summary>
    /// <response code="200">Returns paginated list of users.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<UserListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsersListAsync([FromBody] GetUserListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetUserList>(parameters);
    }

    /// <summary>
    /// Get detailed information about user.
    /// </summary>
    /// <response code="200">Returns detailed information about user.</response>
    /// <response code="404">User not found in database.</response>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetailAsync(ulong id)
        => await ProcessAsync<GetUserDetail>(id);

    /// <summary>
    /// Get data about currently logged user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Only for users with User permissions.</remarks>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(UserDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUserDetailAsync()
        => await ProcessAsync<GetUserDetail>();

    /// <summary>
    /// Update user.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserAsync(ulong id, [FromBody] UpdateUserParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateUser>(id, parameters);
    }

    /// <summary>
    /// Heartbeat event to set that the user is no longer logged in to the administration.
    /// </summary>
    /// <remarks>
    /// Every API call will reenable information about logged state in to the administration.
    /// </remarks>
    [HttpDelete("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> HearthbeatOffAsync()
        => await ProcessAsync<Hearthbeat>(false);

    /// <summary>
    /// Get rubbergod karma leaderboard.
    /// </summary>
    /// <response code="200">Returns paginated response of karma leaderboard</response>
    /// <response code="500">Something is wrong.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPost("karma")]
    [ProducesResponseType(typeof(PaginatedResponse<KarmaListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRubbergodUserKarmaAsync([FromBody] KarmaListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetRubbergodUserKarma>(parameters);
    }

    /// <summary>
    /// Store rubbergod karma info.
    /// </summary>
    /// <param name="items">List of users with current information about karma.</param>
    /// <response code="200">Operation is success.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="500">Something went wrong.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPost("karma/store")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> StoreKarmaAsync([FromBody] List<RawKarmaItem> items)
    {
        ApiAction.Init(this, items.ToArray());
        return ProcessAsync<StoreKarma>(items);
    }

    /// <summary>
    /// Get birthday info for today.
    /// </summary>
    /// <response code="200">Returns formated string with birthdays.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpGet("birthday/today")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayBirthdayInfoAsync()
        => await ProcessAsync<GetTodayBirthdayInfo>();

    /// <summary>
    /// Get information about guild user.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="userId">User ID</param>
    /// <response code="200">Returns info about guild user.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpGet("info/{guildId}/{userId}")]
    [ProducesResponseType(typeof(GuildUserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGuildUserInfoAsync(string guildId, string userId)
        => await ProcessAsync<GetGuildUserInfo>(guildId, userId);
}
