using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using GrillBot.Data.Models.API.Help;
using Microsoft.AspNetCore.Http;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.Data.Exceptions;
using GrillBot.App.Services.User;
using GrillBot.App.Infrastructure.Auth;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/users")]
[OpenApiTag("Users", Description = "User management")]
public class UsersController : Controller
{
    private CommandsHelpService HelpService { get; }
    private ExternalCommandsHelpService ExternalCommandsHelpService { get; }
    private UsersApiService ApiService { get; }
    private RubbergodKarmaService KarmaService { get; }

    public UsersController(CommandsHelpService helpService, ExternalCommandsHelpService externalCommandsHelpService,
        UsersApiService apiService, RubbergodKarmaService karmaService)
    {
        HelpService = helpService;
        ExternalCommandsHelpService = externalCommandsHelpService;
        ApiService = apiService;
        KarmaService = karmaService;
    }

    /// <summary>
    /// Get paginated list of users.
    /// </summary>
    /// <response code="200">Returns paginated list of users.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<UserListItem>>> GetUsersListAsync([FromQuery] GetUserListParams parameters,
        CancellationToken cancellationToken = default)
    {
        var result = await ApiService.GetListAsync(parameters, cancellationToken);
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
    public async Task<ActionResult<UserDetail>> GetUserDetailAsync(ulong id, CancellationToken cancellationToken = default)
    {
        var result = await ApiService.GetUserDetailAsync(id, cancellationToken);

        if (result == null)
            return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

        return Ok(result);
    }

    /// <summary>
    /// Gets data about currently logged user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Only for users with User permissions.</remarks>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserDetail>> GetCurrentUserDetailAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = User.GetUserId();
        var user = await GetUserDetailAsync(currentUserId, cancellationToken);

        if (user.Result is NotFoundObjectResult)
            return user;

        // Remove private data. User not have permission to view this.
        if (user.Result is OkObjectResult okResult && okResult.Value is UserDetail userDetail)
        {
            userDetail.RemoveSecretData();
            return user;
        }

        throw new InvalidOperationException("Při načítání aktuálně přihlášeného uživatele došlo k neočekávanému výstupu.");
    }

    /// <summary>
    /// Gets non paginated list of user available text commands.
    /// </summary>
    [HttpGet("me/commands")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableCommandsAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = User.GetUserId();
        var result = await HelpService.GetHelpAsync(currentUserId, cancellationToken);
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
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableExternalCommandsAsync(string service, CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = User.GetUserId();
            service = char.ToUpper(service[0]).ToString() + service[1..].ToLower();
            var result = await ExternalCommandsHelpService.GetHelpAsync(service, currentUserId, cancellationToken);
            return Ok(result);
        }
        catch (GrillBotException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponse(ex.Message));
        }
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
    public async Task<ActionResult> UpdateUserAsync(ulong id, UpdateUserParams parameters)
    {
        try
        {
            await ApiService.UpdateUserAsync(id, parameters, User);
            return Ok();
        }
        catch (NotFoundException)
        {
            return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));
        }
    }

    /// <summary>
    /// Heartbeat event to set the user to be logged in to the administration.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPost("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatAsync()
    {
        await ApiService.SetHearthbeatStatusAsync(User, true);
        return Ok();
    }

    /// <summary>
    /// Heartbeat event to set that the user is no longer logged in to the administration.
    /// </summary>
    [HttpDelete("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatOffAsync()
    {
        await ApiService.SetHearthbeatStatusAsync(User, false);
        return Ok();
    }

    /// <summary>
    /// Gets complete list of user points.
    /// </summary>
    /// <response code="200">Returns full points board.</response>
    [HttpGet("points/board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<UserPointsItem>>> GetPointsLeaderboardAsync(CancellationToken cancellationToken = default)
    {
        var result = await ApiService.GetPointsBoardAsync(User, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get rubbergod karma leaderboard.
    /// </summary>
    /// <response code="200">Returns paginated response of karma leaderboard</response>
    /// <response code="500">Something is wrong.</response>
    [ApiKeyAuth]
    [HttpGet("karma")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<PaginatedResponse<UserKarma>>> GetRubbergodUserKarmaAsync([FromQuery] KarmaListParams parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await KarmaService.GetUserKarmaAsync(parameters.Sort, parameters.Pagination, cancellationToken);
            return Ok(result);
        }
        catch (GrillBotException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new MessageResponse(ex.Message));
        }
    }
}
