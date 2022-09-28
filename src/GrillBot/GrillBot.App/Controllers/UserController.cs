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
using GrillBot.App.Services.User;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Common.Models;
using GrillBot.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/user")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class UsersController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public UsersController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
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

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.User.GetUserList>();
        var result = await action.ProcessAsync(parameters);

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.User.GetUserDetail>();
        var result = await action.ProcessAsync(id);

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.User.GetUserDetail>();
        var result = await action.ProcessSelfAsync();

        return Ok(result);
    }

    /// <summary>
    /// Gets non paginated list of user available text commands.
    /// </summary>
    [HttpGet("me/commands")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableCommandsAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.GetCommandsHelp>();
        var result = await action.ProcessAsync();

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
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableExternalCommandsAsync(string service)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.GetExternalCommands>();
        var result = await action.ProcessAsync(service);

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

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.User.UpdateUser>();
        await action.ProcessAsync(id, parameters);

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
        var service = ServiceProvider.GetRequiredService<UserHearthbeatService>();
        var apiContext = ServiceProvider.GetRequiredService<ApiRequestContext>();

        await service.UpdateHearthbeatAsync(false, apiContext);
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

        var action = ServiceProvider.GetRequiredService<GetRubbergodUserKarma>();
        var result = await action.ProcessAsync(parameters);

        return Ok(result);
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
        var action = ServiceProvider.GetRequiredService<GetTodayBirthdayInfo>();
        var result = await action.ProcessAsync();

        return Ok(new MessageResponse(result));
    }
}
