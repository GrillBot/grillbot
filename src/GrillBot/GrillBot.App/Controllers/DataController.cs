using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/data")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [OpenApiTag("Data", Description = "Support for form fields, ...")]
    public class DataController : ControllerBase
    {
        private DiscordSocketClient DiscordClient { get; }
        private GrillBotContext DbContext { get; }
        private CommandService CommandService { get; }

        public DataController(DiscordSocketClient discordClient, GrillBotContext dbContext, CommandService commandService)
        {
            DiscordClient = discordClient;
            DbContext = dbContext;
            CommandService = commandService;
        }

        [HttpGet("guilds")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetAvailableGuildsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetAvailableGuildsAsync()
        {
            var guilds = await DbContext.Guilds.AsNoTracking()
                .OrderBy(o => o.Name)
                .Select(o => new { o.Id, o.Name })
                .ToDictionaryAsync(o => o.Id, o => o.Name);

            return Ok(guilds);
        }

        /// <summary>
        /// Get channels
        /// </summary>
        /// <param name="guildId">Optional guild ID</param>
        [HttpGet("channels")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetChannelsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetChannelsAsync(ulong? guildId)
        {
            var guilds = DiscordClient.Guilds.AsEnumerable();
            if (guildId != null) guilds = guilds.Where(o => o.Id == guildId.Value);

            var channels = guilds.SelectMany(o => o.Channels.Select(o => new Channel(o)))
                .Where(o => o.Type != null && o.Type != ChannelType.Category).ToList();

            var dbChannelsQuery = DbContext.Channels.AsNoTracking()
                .Where(o => o.ChannelType != ChannelType.Category)
                .OrderBy(o => o.Name).AsQueryable();

            if (guildId != null)
                dbChannelsQuery = dbChannelsQuery.Where(o => o.GuildId == guildId.ToString());

            var query = dbChannelsQuery.Select(o => new Channel()
            {
                Type = o.ChannelType,
                Id = o.ChannelId,
                Name = o.Name
            });

            var dbChannels = (await query.ToListAsync())
                .Where(o => !channels.Any(x => x.Id == o.Id));

            channels.AddRange(dbChannels);

            var result = channels.OrderBy(o => o.Name).ToDictionary(o => o.Id, o => o.Name);
            return Ok(result);
        }

        /// <summary>
        /// Get roles
        /// </summary>
        [HttpGet("roles")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetRoles))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<Dictionary<string, string>> GetRoles(ulong? guildId)
        {
            var guilds = DiscordClient.Guilds.AsEnumerable();
            if (guildId != null) guilds = guilds.Where(o => o.Id == guildId.Value);

            var roles = guilds.SelectMany(o => o.Roles)
                .Where(o => !o.IsEveryone)
                .OrderBy(o => o.Name)
                .ToDictionary(o => o.Id.ToString(), o => o.Name);

            return Ok(roles);
        }

        /// <summary>
        /// Get non-paginated commands list
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("commands")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetCommandsList))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<string>> GetCommandsList()
        {
            var commands = CommandService.Commands
                .Select(o => o.Aliases[0]?.Trim())
                .OrderBy(o => o)
                .Distinct()
                .ToList();

            return Ok(commands);
        }

        /// <summary>
        /// Gets non-paginated list of users.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("users")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetAvailableUsersAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetAvailableUsersAsync()
        {
            var users = await DbContext.Users.AsNoTracking()
                .Where(o => (o.Flags & (int)UserFlags.NotUser) == 0)
                .Select(o => new { o.Id, o.Username })
                .OrderBy(o => o.Username)
                .ToDictionaryAsync(o => o.Id, o => o.Username);

            return Ok(users);
        }
    }
}
