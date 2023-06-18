using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V2;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Common.Services.RubbergodService;
using GrillBot.Common.Services.RubbergodService.Models.Karma;
using GrillBot.Core.Models.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user")]
[ApiExplorerSettings(GroupName = "v1")]
public class UsersController : Infrastructure.ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<UserListItem>>> GetUsersListAsync([FromBody] GetUserListParams parameters)
    {
        ApiAction.Init(this, parameters);
        parameters.FixStatus();

        return Ok(await ProcessActionAsync<Actions.Api.V1.User.GetUserList, PaginatedResponse<UserListItem>>(action => action.ProcessAsync(parameters)));
    }

    /// <summary>
    /// Get detailed information about user.
    /// </summary>
    /// <response code="200">Returns detailed information about user.</response>
    /// <response code="404">User not found in database.</response>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetail>> GetUserDetailAsync(ulong id)
        => Ok(await ProcessActionAsync<Actions.Api.V1.User.GetUserDetail, UserDetail>(action => action.ProcessAsync(id)));

    /// <summary>
    /// Get data about currently logged user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Only for users with User permissions.</remarks>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserDetail>> GetCurrentUserDetailAsync()
        => Ok(await ProcessActionAsync<Actions.Api.V1.User.GetUserDetail, UserDetail>(action => action.ProcessSelfAsync()));

    /// <summary>
    /// Update user.
    /// </summary>
    /// <response code="200"></response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">User not found.</response>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> UpdateUserAsync(ulong id, [FromBody] UpdateUserParams parameters)
    {
        ApiAction.Init(this, parameters);

        await ProcessActionAsync<Actions.Api.V1.User.UpdateUser>(action => action.ProcessAsync(id, parameters));
        return Ok();
    }

    /// <summary>
    /// Heartbeat event to set that the user is no longer logged in to the administration.
    /// </summary>
    /// <remarks>
    /// Every API call will reenable information about logged state in to the administration.
    /// </remarks>
    [HttpDelete("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatOffAsync()
    {
        var apiContext = ServiceProvider.GetRequiredService<ApiRequestContext>();
        await ProcessActionAsync<UserManager>(manager => manager.SetHearthbeatAsync(false, apiContext));
        return Ok();
    }

    /// <summary>
    /// Get rubbergod karma leaderboard.
    /// </summary>
    /// <response code="200">Returns paginated response of karma leaderboard</response>
    /// <response code="500">Something is wrong.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPost("karma")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PaginatedResponse<UserKarma>>> GetRubbergodUserKarmaAsync([FromBody] KarmaListParams parameters)
    {
        ApiAction.Init(this, parameters);

        return Ok(await ProcessBridgeAsync<IRubbergodServiceClient, PaginatedResponse<UserKarma>>(client => client.GetKarmaPageAsync(parameters.Pagination)));
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
    public async Task<ActionResult> StoreKarmaAsync([FromBody] List<KarmaItem> items)
    {
        ApiAction.Init(this, items.ToArray());

        await ProcessBridgeAsync<IRubbergodServiceClient>(client => client.StoreKarmaAsync(items));
        return Ok();
    }

    /// <summary>
    /// Get birthday info for today.
    /// </summary>
    /// <response code="200">Returns formated string with birthdays.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpGet("birthday/today")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponse>> GetTodayBirthdayInfoAsync()
        => Ok(await ProcessActionAsync<GetTodayBirthdayInfo, MessageResponse>(action => action.ProcessAsync()));
}
