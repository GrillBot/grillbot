using GrillBot.App.Extensions.Discord;
using GrillBot.App.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using GrillBot.Database.Enums;
using GrillBot.Database.Entity;
using GrillBot.Data.Models.API.Help;
using Microsoft.AspNetCore.Http;
using GrillBot.App.Services.CommandsHelp;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/users")]
[OpenApiTag("Users", Description = "User management")]
public class UsersController : Controller
{
    private GrillBotContext DbContext { get; }
    private DiscordSocketClient DiscordClient { get; }
    private CommandsHelpService HelpService { get; }
    private ExternalCommandsHelpService ExternalCommandsHelpService { get; }

    public UsersController(GrillBotContext dbContext, DiscordSocketClient discordClient, CommandsHelpService helpService,
        ExternalCommandsHelpService externalCommandsHelpService)
    {
        DbContext = dbContext;
        DiscordClient = discordClient;
        HelpService = helpService;
        ExternalCommandsHelpService = externalCommandsHelpService;
    }

    /// <summary>
    /// Gets paginated list of users.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetUsersListAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<UserListItem>>> GetUsersListAsync([FromQuery] GetUserListParams parameters,
        CancellationToken cancellationToken)
    {
        var query = DbContext.Users.AsNoTracking().AsSplitQuery()
            .Include(o => o.Guilds).ThenInclude(o => o.Guild)
            .AsQueryable();

        query = parameters.CreateQuery(query);
        var result = await PaginatedResponse<UserListItem>.CreateAsync(query, parameters, async (entity, cancellationToken) =>
        {
            var discordUser = await DiscordClient.FindUserAsync(Convert.ToUInt64(entity.Id), cancellationToken);
            return new UserListItem(entity, DiscordClient, discordUser);
        }, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets detailed information about user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">User not found in database.</response>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetUserDetailAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserDetail>> GetUserDetailAsync(ulong id, CancellationToken cancellationToken)
    {
        var query = DbContext.Users.AsNoTracking()
            .Include(o => o.Guilds).ThenInclude(o => o.Guild)
            .Include(o => o.Guilds).ThenInclude(o => o.UsedInvite).ThenInclude(o => o.Creator).ThenInclude(o => o.User)
            .Include(o => o.Guilds).ThenInclude(o => o.CreatedInvites)
            .Include(o => o.Guilds).ThenInclude(o => o.Channels).ThenInclude(o => o.Channel)
            .Include(o => o.UsedEmotes)
            .AsSplitQuery();

        var entity = await query.FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);

        if (entity == null)
            return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

        var user = await DiscordClient.FindUserAsync(id, cancellationToken);
        var detail = new UserDetail(entity, user, DiscordClient);
        return Ok(detail);
    }

    /// <summary>
    /// Gets data about currently logged user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">User not found.</response>
    /// <remarks>Only for users with User permissions.</remarks>
    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetCurrentUserDetailAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserDetail>> GetCurrentUserDetailAsync(CancellationToken cancellationToken)
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
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetAvailableCommandsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CommandGroup>>> GetAvailableCommandsAsync(CancellationToken cancellationToken)
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
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetAvailableExternalCommandsAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
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
    /// Updates user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">User not found</response>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(UpdateUserAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<UserDetail>> UpdateUserAsync(ulong id, UpdateUserParams parameters, CancellationToken cancellationToken)
    {
        var user = await DbContext.Users.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);

        if (user == null)
            return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

        user.Note = parameters.Note;
        user.SelfUnverifyMinimalTime = parameters.SelfUnverifyMinimalTime;

        if (parameters.BotAdmin)
            user.Flags |= (int)UserFlags.BotAdmin;
        else
            user.Flags &= ~(int)UserFlags.BotAdmin;

        if (parameters.WebAdminAllowed)
            user.Flags |= (int)UserFlags.WebAdmin;
        else
            user.Flags &= ~(int)UserFlags.WebAdmin;

        if (parameters.PublicAdminBlocked)
            user.Flags |= (int)UserFlags.PublicAdministrationBlocked;
        else
            user.Flags &= ~(int)UserFlags.PublicAdministrationBlocked;

        var userId = User.GetUserId();
        var discordUser = await DiscordClient.FindUserAsync(userId, cancellationToken);
        await DbContext.InitUserAsync(discordUser, cancellationToken);

        var logItem = AuditLogItem.Create(AuditLogItemType.Info, null, null, discordUser,
            $"Uživatel {user.Username}#{user.Discriminator} byl aktualizován (Flags:{user.Flags},Note:{user.Note})", null);

        await DbContext.AddAsync(logItem, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        return await GetUserDetailAsync(id, cancellationToken);
    }

    /// <summary>
    /// Heartbeat event to set the user to be logged in to the administration.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPost("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(HearthbeatAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatAsync(CancellationToken cancellationToken)
    {
        await SetWebAdminStatusAsync(true, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Heartbeat event to set that the user is no longer logged in to the administration.
    /// </summary>
    [HttpDelete("hearthbeat")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(HearthbeatOffAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult> HearthbeatOffAsync(CancellationToken cancellationToken)
    {
        await SetWebAdminStatusAsync(false, cancellationToken);
        return Ok();
    }

    private async Task SetWebAdminStatusAsync(bool isOnline, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId().ToString();
        var isPublic = User.HaveUserPermission();

        var user = await DbContext.Users.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == userId, cancellationToken);

        if (isOnline)
            user.Flags |= (int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);
        else
            user.Flags &= ~(int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Gets complete list of user points.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("points/board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetPointsLeaderboardAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserPointsItem>> GetPointsLeaderboardAsync(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = new List<UserPointsItem>();
        var mutualGuildIds = DiscordClient.FindMutualGuilds(userId).Select(o => o.Id.ToString()).ToList();

        var guildUsersQuery = DbContext.GuildUsers.AsNoTracking()
            .Include(o => o.Guild)
            .Include(o => o.User)
            .Where(o =>
                o.Points > 0 &&
                (o.User.Flags & (long)UserFlags.NotUser) == 0 &&
                mutualGuildIds.Contains(o.GuildId) &&
                !o.User.Username.StartsWith("Imported")
            )
            .OrderByDescending(o => o.Points)
            .ThenBy(o => o.User.Username);

        var guildUsersResult = await guildUsersQuery.ToListAsync(cancellationToken);
        if (guildUsersResult.Count > 0)
            result.AddRange(guildUsersResult.ConvertAll(o => new UserPointsItem(o)));

        return Ok(result);
    }
}
