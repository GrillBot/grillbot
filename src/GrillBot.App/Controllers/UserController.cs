using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V2;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Data.Models.API.Help;
using Microsoft.AspNetCore.Http;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Common.Models.Pagination;
using GrillBot.Common.Services.RubbergodService.Models.Karma;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
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

        var result = await ProcessActionAsync<Actions.Api.V1.User.GetUserList, PaginatedResponse<UserListItem>>(action => action.ProcessAsync(parameters));
        return Ok(result);
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
    {
        var result = await ProcessActionAsync<Actions.Api.V1.User.GetUserDetail, UserDetail>(action => action.ProcessAsync(id));
        return Ok(result);
    }

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
    {
        var result = await ProcessActionAsync<Actions.Api.V1.User.GetUserDetail, UserDetail>(action => action.ProcessSelfAsync());
        return Ok(result);
    }

    /// <summary>
    /// Get non paginated list of available commands from external service.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="500">Something is wrong</response>
    [HttpGet("me/commands/{service}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableExternalCommandsAsync(string service)
    {
        var result = await ProcessActionAsync<Actions.Api.V1.Command.GetExternalCommands, List<CommandGroup>>(action => action.ProcessAsync(service));
        return Ok(result);
    }

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

        var result = await ProcessActionAsync<GetRubbergodUserKarma, PaginatedResponse<UserKarma>>(action => action.ProcessAsync(parameters));
        return Ok(result);
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

        await ProcessActionAsync<StoreKarma>(action => action.ProcessAsync(items));
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
    {
        var result = await ProcessActionAsync<GetTodayBirthdayInfo, string>(action => action.ProcessAsync());
        return Ok(new MessageResponse(result));
    }
}
