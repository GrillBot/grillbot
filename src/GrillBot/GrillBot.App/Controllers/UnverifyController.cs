using Discord.WebSocket;
using GrillBot.App.Services.Unverify;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Unverify;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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
        /// <param name="guildId">Guild ID</param>
        /// <response code="200">Success</response>
        [HttpGet("{guildId}/current")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(GetCurrentUnverifiesAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<List<UnverifyUserProfile>>> GetCurrentUnverifiesAsync(ulong guildId)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return NotFound(new MessageResponse("Požadovaný server nebyl nalezen."));

            var unverifies = await UnverifyService.GetAllUnverifiesOfGuildAsync(guild);
            var result = unverifies.ConvertAll(o => new UnverifyUserProfile(o));
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

            var fromUser = guild.CurrentUser; // TODO: Logged user
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

            var result = await UnverifyService.UpdateUnverifyAsync(toUser, guild, endTime, guild.CurrentUser); // TODO: Logged user
            return Ok(new MessageResponse(result));
        }

        /// <summary>
        /// Gets paginated list of unverify logs.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("log")]
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(GetUnverifLogsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<UnverifyLogItem>>> GetUnverifLogsAsync([FromQuery] UnverifyLogParams parameters)
        {
            var query = DbContext.UnverifyLogs.AsNoTracking()
                .Include(o => o.FromUser)
                .ThenInclude(o => o.User)
                .Include(o => o.Guild)
                .Include(o => o.ToUser)
                .ThenInclude(o => o.User)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<UnverifyLogItem>.CreateAsync(query, parameters, entity => new UnverifyLogItem(entity));
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
        [OpenApiOperation(nameof(UnverifyController) + "_" + nameof(GetUnverifLogsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> RecoverUnverifyAsync(long logId)
        {
            try
            {
                await UnverifyService.RecoverUnverifyState(logId, DiscordClient.CurrentUser.Id); // TODO: Logged user 
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
