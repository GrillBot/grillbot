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
using GrillBot.App.Services;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/users")]
    [OpenApiTag("Users", Description = "User management")]
    public class UsersController : Controller
    {
        private GrillBotContext DbContext { get; }
        private DiscordSocketClient DiscordClient { get; }
        private HelpService HelpService { get; }

        public UsersController(GrillBotContext dbContext, DiscordSocketClient discordClient, HelpService helpService)
        {
            DbContext = dbContext;
            DiscordClient = discordClient;
            HelpService = helpService;
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
        public async Task<ActionResult<PaginatedResponse<UserListItem>>> GetUsersListAsync([FromQuery] GetUserListParams parameters)
        {
            var query = DbContext.Users.AsNoTracking().AsSplitQuery()
                .Include(o => o.Guilds).ThenInclude(o => o.Guild)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<UserListItem>.CreateAsync(query, parameters, async entity =>
            {
                var discordUser = await DiscordClient.FindUserAsync(Convert.ToUInt64(entity.Id));
                return new(entity, DiscordClient, discordUser);
            });
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
        public async Task<ActionResult<UserDetail>> GetUserDetailAsync(ulong id)
        {
            var query = DbContext.Users.AsNoTracking()
                .Include(o => o.Guilds).ThenInclude(o => o.Guild)
                .Include(o => o.Guilds).ThenInclude(o => o.UsedInvite).ThenInclude(o => o.Creator).ThenInclude(o => o.User)
                .Include(o => o.Guilds).ThenInclude(o => o.CreatedInvites)
                .Include(o => o.Guilds).ThenInclude(o => o.Channels).ThenInclude(o => o.Channel)
                .Include(o => o.UsedEmotes)
                .AsSplitQuery();

            var entity = await query.FirstOrDefaultAsync(o => o.Id == id.ToString());

            if (entity == null)
                return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

            var user = await DiscordClient.FindUserAsync(id);
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
        public async Task<ActionResult<UserDetail>> GetCurrentUserDetailAsync()
        {
            var currentUserId = User.GetUserId();
            var user = await GetUserDetailAsync(currentUserId);

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
        public async Task<ActionResult<List<CommandGroup>>> GetAvailableCommandsAsync()
        {
            var currentUserId = User.GetUserId();
            var result = await HelpService.GetHelpAsync(currentUserId);
            return Ok(result);
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
        public async Task<ActionResult<UserDetail>> UpdateUserAsync(ulong id, UpdateUserParams parameters)
        {
            var user = await DbContext.Users.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id.ToString());

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
            var discordUser = await DiscordClient.FindUserAsync(userId);

            await DbContext.InitUserAsync(discordUser, CancellationToken.None);

            var logItem = AuditLogItem.Create(AuditLogItemType.Info, null, null, discordUser,
                $"Uživatel {user.Username} byl aktualizován (Flags:{user.Flags},Note:{user.Note})");

            await DbContext.AddAsync(logItem);
            await DbContext.SaveChangesAsync();
            return await GetUserDetailAsync(id);
        }

        /// <summary>
        /// Heartbeat event to set the user to be logged in to the administration.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpPost("hearthbeat")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [OpenApiOperation(nameof(UsersController) + "_" + nameof(HearthbeatAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> HearthbeatAsync(bool isPublic)
        {
            await SetWebAdminStatusAsync(true, isPublic);
            return Ok();
        }

        /// <summary>
        /// Heartbeat event to set that the user is no longer logged in to the administration.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("hearthbeat")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User,Admin")]
        [OpenApiOperation(nameof(UsersController) + "_" + nameof(HearthbeatOffAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult> HearthbeatOffAsync(bool isPublic)
        {
            await SetWebAdminStatusAsync(false, isPublic);
            return Ok();
        }

        private async Task SetWebAdminStatusAsync(bool isOnline, bool isPublic)
        {
            var userId = User.GetUserId().ToString();
            var user = await DbContext.Users.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == userId);

            if (isOnline)
                user.Flags |= (int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);
            else
                user.Flags &= ~(int)(isPublic ? UserFlags.PublicAdminOnline : UserFlags.WebAdminOnline);

            await DbContext.SaveChangesAsync();
        }
    }
}
