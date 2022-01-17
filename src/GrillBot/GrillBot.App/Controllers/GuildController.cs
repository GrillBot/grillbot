using Discord.WebSocket;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.Data.Controllers
{
    [ApiController]
    [Route("api/guild")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("Guilds", Description = "Guild management")]
    public class GuildController : Controller
    {
        private GrillBotContext DbContext { get; }
        private DiscordSocketClient DiscordClient { get; }

        public GuildController(GrillBotContext dbContext, DiscordSocketClient discordClient)
        {
            DbContext = dbContext;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets paginated list of guilds.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet]
        [OpenApiOperation(nameof(GuildController) + "_" + nameof(GetGuildListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<Guild>>> GetGuildListAsync([FromQuery] GetGuildListParams parameters)
        {
            var query = parameters.CreateQuery(
                DbContext.Guilds.AsNoTracking().OrderBy(o => o.Name)
            );
            var result = await PaginatedResponse<Guild>.CreateAsync(query, parameters, entity => new Guild(entity));

            foreach (var guildData in result.Data)
            {
                var guild = DiscordClient.GetGuild(Convert.ToUInt64(guildData.Id));
                if (guild == null) continue;

                guildData.MemberCount = guild.MemberCount;
                guildData.IsConnected = guild.IsConnected;
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets detailed information about guild.
        /// </summary>
        /// <param name="id">Guild ID</param>
        [HttpGet("{id}")]
        [OpenApiOperation(nameof(GuildController) + "_" + nameof(GetGuildDetailAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<GuildDetail>> GetGuildDetailAsync(ulong id)
        {
            var dbGuild = await DbContext.Guilds.AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id.ToString());

            if (dbGuild == null)
                return NotFound(new MessageResponse("Nepodařilo se dohledat server."));

            var guild = DiscordClient.GetGuild(id);
            return Ok(new GuildDetail(guild, dbGuild));
        }

        /// <summary>
        /// Updates guild
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Guild not found.</response>
        [HttpPut("{id}")]
        [OpenApiOperation(nameof(GuildController) + "_" + nameof(GetGuildDetailAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<GuildDetail>> UpdateGuildAsync(ulong id, [FromBody] UpdateGuildParams updateGuildParams)
        {
            var guild = DiscordClient.GetGuild(id);

            if (guild == null)
                return NotFound(new MessageResponse("Nepodařilo se dohledat server."));

            var dbGuild = await DbContext.Guilds.AsQueryable()
                .FirstOrDefaultAsync(o => o.Id == id.ToString());

            if (updateGuildParams.AdminChannelId != null && guild.GetTextChannel(Convert.ToUInt64(updateGuildParams.AdminChannelId)) == null)
            {
                var details = new ValidationProblemDetails(new Dictionary<string, string[]>()
                {
                    { nameof(updateGuildParams.AdminChannelId), new[]{ "Nepodařilo se dohledat zadaný administrátorský kanál" } }
                });

                return BadRequest(details);
            }
            else
            {
                dbGuild.AdminChannelId = updateGuildParams.AdminChannelId;
            }

            if (updateGuildParams.MuteRoleId != null && guild.GetRole(Convert.ToUInt64(updateGuildParams.MuteRoleId)) == null)
            {
                var details = new ValidationProblemDetails(new Dictionary<string, string[]>()
                {
                    { nameof(updateGuildParams.MuteRoleId), new[]{ "Nepodařilo se dohledat roli, která reprezentuje umlčení uživatele při unverify." } }
                });

                return BadRequest(details);
            }
            else
            {
                dbGuild.MuteRoleId = updateGuildParams.MuteRoleId;
            }

            await DbContext.SaveChangesAsync();
            return Ok(new GuildDetail(guild, dbGuild));
        }
    }
}
