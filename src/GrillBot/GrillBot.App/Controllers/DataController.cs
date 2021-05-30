using Discord.WebSocket;
using GrillBot.Data.Models.API;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/data")]
    [OpenApiTag("Data", Description = "Support controller for form controls, ...")]
    public class DataController : ControllerBase
    {
        private DiscordSocketClient DiscordClient { get; }

        public DataController(DiscordSocketClient discordClient)
        {
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Gets nonpaginated list of guilds.
        /// </summary>
        /// <response code="200">OK</response>
        [HttpGet("guilds")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<Guild>> GetGuilds()
        {
            return Ok(DiscordClient.Guilds.Select(o => new Guild(o)));
        }

        /// <summary>
        /// Gets list of channels in guild ordered by position.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <response code="200">OK</response>
        /// <response code="404">Guild not found</response>
        [HttpGet("channels/{guildId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public ActionResult<List<GuildChannel>> GetChannelsOfGuild(ulong guildId)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return NotFound(new MessageResponse($"Server s ID {guildId} nebyl nalezen."));

            var channels = guild.Channels
                .Where(o => guild.GetCategoryChannel(o.Id) == null)
                .OrderByDescending(o => o.Position)
                .Select(o => new GuildChannel(o));

            return Ok(channels);
        }

        /// <summary>
        /// Gets list of users in guild ordered by username and nickname.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        /// <response code="200">OK</response>
        /// <response code="404">Guild not found.</response>
        [HttpGet("users/{guildId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<List<User>>> GetUsersOfGuild(ulong guildId)
        {
            var guild = DiscordClient.GetGuild(guildId);

            if (guild == null)
                return NotFound(new MessageResponse($"Server s ID {guildId} nebyl nalezen."));

            await guild.DownloadUsersAsync();

            var users = guild.Users
                .OrderBy(o => o.Username)
                .ThenBy(o => o.DiscriminatorValue)
                .ThenByDescending(o => o.Hierarchy)
                .Select(o => new User(o));

            return Ok(users);
        }
    }
}
