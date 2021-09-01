using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/users")]
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
                .Include(o => o.Guilds).ThenInclude(o => o.UsedInvite)
                .Include(o => o.Guilds).ThenInclude(o => o.CreatedInvites)
                .Include(o => o.Guilds).ThenInclude(o => o.Channels).ThenInclude(o => o.Channel)
                .Include(o => o.UsedEmotes)
                .AsSplitQuery();

            var entity = await query.FirstOrDefaultAsync(o => o.Id == id.ToString());

            if (entity == null)
                return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

            var user = await DiscordClient.FindUserAsync(id);
            var detail = new UserDetail(entity, user);
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

            // TODO: Processed user
            var logItem = AuditLogItem.Create(AuditLogItemType.Info, null, null, DiscordClient.CurrentUser,
                $"Uživatel {user.Username} byl aktualizován (Flags:{user.Flags},ApiToken:{user.ApiToken},Note:{user.Note})");

            await DbContext.AddAsync(logItem);
            await DbContext.SaveChangesAsync();
            return await GetUserDetailAsync(id);
        }

        /// <summary>
        /// Increment points for user on server.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Guild not found or user is not found in guild.</response>
        [HttpPost("{userId}/{guildId}")]
        [OpenApiOperation(nameof(UsersController) + "_" + nameof(IncrementPointsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> IncrementPointsAsync(ulong userId, ulong guildId, [Required] long amount)
        {
            var user = await DbContext.Users.AsQueryable()
                .Include(o => o.Guilds)
                .ThenInclude(o => o.Guild)
                .FirstOrDefaultAsync(o => o.Id == userId.ToString());

            if (user == null)
                return NotFound(new MessageResponse("Zadaný uživatel nebyl nalezen."));

            var guildUser = user.Guilds.FirstOrDefault(o => o.GuildId == guildId.ToString());
            if (guildUser == null)
                return NotFound(new MessageResponse($"Uživatel {user.Username} nemá záznam o přítomnosti na zadaném serveru."));

            guildUser.Points += amount;

            var guild = DiscordClient.GetGuild(guildId);
            if (guild != null) await DbContext.InitGuildAsync(guild);
            // TODO: Processed user
            var logItem = AuditLogItem.Create(AuditLogItemType.Info, guild, null, DiscordClient.CurrentUser,
                $"Uživatel {user.Username} obdržel na serveru {guildUser.Guild.Name} body ({amount})");

            await DbContext.AddAsync(logItem);
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Transfers points between users on server.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Users not found or not found guild.</response>
        [HttpPut("{guildId}/{fromUserId}/{toUserId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> TransferPointsAsync(ulong guildId, ulong fromUserId, ulong toUserId, [Required] long amount)
        {
            if (!(await DbContext.Guilds.AsQueryable().AnyAsync(o => o.Id == guildId.ToString())))
                return NotFound(new MessageResponse("Zadaný server nebyl nalezen."));

            var fromUser = await DbContext.GuildUsers.AsQueryable()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == fromUserId.ToString() && o.GuildId == guildId.ToString());

            if (fromUser == null)
                return NotFound(new MessageResponse("Odesílatel bodů nebyl na serveru nalezen."));

            var toUser = await DbContext.GuildUsers.AsQueryable()
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.UserId == toUserId.ToString() && o.GuildId == guildId.ToString());

            if (toUser == null)
                return NotFound(new MessageResponse("Příjemce bodů nebyl na serveru nalezen."));

            if (fromUser.Points < amount)
            {
                var errors = new Dictionary<string, string[]>()
                {
                    { "PointsBalance", new[] { $"Uživatel {fromUser.Nickname ?? fromUser.User.Username} nemá na účtu dodatek bodů k převodu." } }
                };

                return BadRequest(new ValidationProblemDetails(errors));
            }

            fromUser.Points -= amount;
            toUser.Points += amount;

            var guild = DiscordClient.GetGuild(guildId);
            if (guild != null) await DbContext.InitGuildAsync(guild);
            // TODO: Processed user
            var logItem = AuditLogItem.Create(AuditLogItemType.Info, guild, null, DiscordClient.CurrentUser,
                $"Byly převedeny body uživatele {fromUser.User.Username} uživateli {toUser.User.Username} ({amount})");

            await DbContext.AddAsync(logItem);
            await DbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
