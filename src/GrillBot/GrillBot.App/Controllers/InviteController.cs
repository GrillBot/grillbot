using Discord.WebSocket;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/invite")]
    [OpenApiTag("Invite", Description = "Invite management")]
    public class InviteController : Controller
    {
        private GrillBotContext DbContext { get; }
        private DiscordSocketClient DiscordClient { get; }

        public InviteController(GrillBotContext grillBotContext, DiscordSocketClient discordClient)
        {
            DbContext = grillBotContext;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets paginated list of stored invites.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Guild not found.</response>
        [HttpGet("{guildId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<InviteList>> GetStoredInvitesAsync(ulong guildId, [FromQuery] GetStoredInvitesParams @params)
        {
            var guild = DiscordClient.GetGuild(guildId);
            if (guild == null)
                return NotFound(new MessageResponse($"Server s ID {guildId} nebyl nalezen."));

            await guild.DownloadUsersAsync();
            var baseQuery = DbContext.Invites
                .AsNoTracking()
                .Include(o => o.Creator)
                .Where(o => o.GuildId == guildId.ToString());

            var query = @params.CreateQuery(baseQuery).Select(o => new { Invite = o, UsedUsersCount = o.UsedUsers.Count });

            var list = new InviteList(guild);
            foreach (var row in await query.ToListAsync())
            {
                var invite = new Invite()
                {
                    Code = row.Invite.Code,
                    CreatedAt = row.Invite.CreatedAt,
                    UsedUsersCount = row.UsedUsersCount
                };

                var user = string.IsNullOrEmpty(row.Invite.CreatorId) ? null : guild.GetUser(Convert.ToUInt64(row.Invite.CreatorId));

                if (user != null)
                    invite.Creator = new User(user);

                list.Invites.Add(invite);
            }

            return Ok(list);
        }
    }
}
