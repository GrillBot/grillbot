using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [OpenApiTag("Users", Description = "User management")]
    public class UsersController : Controller
    {
        private GrillBotContext DbContext { get; }
        private DiscordSocketClient DiscordClient { get; }

        public UsersController(GrillBotContext dbContext, DiscordSocketClient discordClient)
        {
            DbContext = dbContext;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets paginated list of users.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet]
        [OpenApiOperation(nameof(UsersController) + "_" + nameof(GetUsersListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<UserListItem>>> GetUsersListAsync([FromQuery] GetUserListParams parameters)
        {
            var query = DbContext.Users.AsNoTracking()
                .Include(o => o.Guilds)
                .ThenInclude(o => o.Guild)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<UserListItem>.CreateAsync(query, parameters, entity => new(entity, DiscordClient));
            return Ok(result);
        }

        /// <summary>
        /// Gets detailed information about user.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="404">User not found in database.</response>
        [HttpGet("{id}")]
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
        /// Updates user.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">User not found</response>
        [HttpPut("{id}")]
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

            user.ApiToken = parameters.ApiToken;
            user.Note = parameters.Note;

            if (parameters.BotAdmin)
                user.Flags |= (int)UserFlags.BotAdmin;
            else
                user.Flags &= ~(int)UserFlags.BotAdmin;

            var userId = User.GetUserId();
            var discordUser = await DiscordClient.FindUserAsync(userId);

            await DbContext.InitUserAsync(discordUser, CancellationToken.None);

            var logItem = AuditLogItem.Create(AuditLogItemType.Info, null, null, discordUser,
                $"Uživatel {user.Username} byl aktualizován (Flags:{user.Flags},ApiToken:{user.ApiToken},Note:{user.Note})");

            await DbContext.AddAsync(logItem);
            await DbContext.SaveChangesAsync();
            return await GetUserDetailAsync(id);
        }
    }
}
