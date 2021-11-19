using Discord.WebSocket;
using GrillBot.App.Extensions;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/unverify")]
    [OpenApiTag("Unverify", Description = "Unverify management.")]
    public class UnverifyController : ControllerBase
    {
        private UnverifyService UnverifyService { get; }
        private DiscordSocketClient DiscordClient { get; }
        private GrillBotContext DbContext { get; }

        public UnverifyController(UnverifyService unverifyService, DiscordSocketClient discordSocketClient,
            GrillBotContext dbContext)
        {
            UnverifyService = unverifyService;
            DiscordClient = discordSocketClient;
            DbContext = dbContext;
        }

        /// <summary>
        /// Gets list of current unverifies in guild.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("current")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(GetCurrentUnverifiesAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<UnverifyUserProfile>>> GetCurrentUnverifiesAsync()
        {
            var userId = User.HaveUserPermission() ? User.GetUserId() : (ulong?)null;

            var unverifies = await UnverifyService.GetAllUnverifiesAsync(userId);
            var result = unverifies.ConvertAll(o => new UnverifyUserProfile(o.Item1, o.Item2));
            return Ok(result);
        }

        /// <summary>
        /// Removes unverify
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <param name="userId">User Id</param>
        /// <response code="200">Success</response>
        /// <response code="404">Unverify or guild not found.</response>
        [HttpDelete("{guildId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(RemoveUnverifyAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<MessageResponse>> RemoveUnverifyAsync(ulong guildId, ulong userId)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return NotFound(new MessageResponse("Server na kterém by se mělo nacházet unverify nebyl nalezen."));

            await guild.DownloadUsersAsync();
            var toUser = guild.GetUser(userId);
            if (toUser == null)
                return NotFound(new MessageResponse("Uživatel, kterému mělo být přiřazeno unverify nebyl nalezen."));

            var fromUserId = User.GetUserId();
            var fromUser = guild.GetUser(fromUserId);
            var result = await UnverifyService.RemoveUnverifyAsync(guild, fromUser, toUser, false);
            return Ok(new MessageResponse(result));
        }

        /// <summary>
        /// Updates unverify time.
        /// </summary>
        /// <param name="guildId">Guild Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="endTime">New unverify end.</param>
        [HttpPut("{guildId}/{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(UpdateUnverifyTimeAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<MessageResponse>> UpdateUnverifyTimeAsync(ulong guildId, ulong userId, [FromQuery, Required] DateTime endTime)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return NotFound(new MessageResponse("Server na kterém by se mělo nacházet unverify nebyl nalezen."));

            await guild.DownloadUsersAsync();
            var toUser = guild.GetUser(userId);
            if (toUser == null)
                return NotFound(new MessageResponse("Uživatel, kterému mělo být přiřazeno unverify nebyl nalezen."));

            var fromUser = guild.GetUser(User.GetUserId());
            var result = await UnverifyService.UpdateUnverifyAsync(toUser, guild, endTime, fromUser);
            return Ok(new MessageResponse(result));
        }

        /// <summary>
        /// Gets paginated list of unverify logs.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("log")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(GetUnverifLogsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<UnverifyLogItem>>> GetUnverifLogsAsync([FromQuery] UnverifyLogParams parameters)
        {
            var query = DbContext.UnverifyLogs.AsNoTracking()
                .Include(o => o.FromUser).ThenInclude(o => o.User)
                .Include(o => o.Guild)
                .Include(o => o.ToUser).ThenInclude(o => o.User)
                .AsQueryable();

            var loggedUserId = User.HaveUserPermission() ? User.GetUserId() : (ulong?)null;

            query = parameters.CreateQuery(query, loggedUserId);
            var result = await PaginatedResponse<UnverifyLogItem>.CreateAsync(query, parameters, entity =>
            {
                var guild = DiscordClient.GetGuild(Convert.ToUInt64(entity.GuildId));
                return new UnverifyLogItem(entity, guild);
            });
            return Ok(result);
        }

        /// <summary>
        /// Recovers state before specific unverify.
        /// </summary>
        /// <param name="logId">ID of log.</param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Unverify, guild or users not found.</response>
        [HttpPost("log/{logId}/recover")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(GetUnverifLogsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> RecoverUnverifyAsync(long logId)
        {
            try
            {
                var processedUserId = User.GetUserId();
                await UnverifyService.RecoverUnverifyState(logId, processedUserId);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new MessageResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                var errors = new ValidationProblemDetails(new Dictionary<string, string[]>()
                {
                    { "Recover", new[] { ex.Message } }
                });

                return BadRequest(errors);
            }

            return Ok();
        }
    }
}
