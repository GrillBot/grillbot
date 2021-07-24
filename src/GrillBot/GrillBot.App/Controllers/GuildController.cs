using Discord.WebSocket;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/guild")]
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
            }

            return Ok(result);
        }
    }
}
