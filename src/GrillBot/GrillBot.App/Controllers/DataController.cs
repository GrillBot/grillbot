using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
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
        /// Get channels of guild.
        /// </summary>
        /// <param name="guildId">Guild ID</param>
        [HttpGet("{guildId}/channels")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetChannelsOfGuild))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetChannelsOfGuild(ulong guildId)
        {
            var guild = DiscordClient.GetGuild(guildId);
            var channels = guild.Channels
                .Select(o => new GuildChannel(o))
                .Where(o => o.Type != null)
                .ToList();

            var dbChannelsQuery = DbContext.Channels.AsNoTracking()
                .Where(o => o.GuildId == guildId.ToString())
                .OrderBy(o => o.Name)
                .Select(o => new GuildChannel()
                {
                    Type = o.ChannelType,
                    Id = o.ChannelId,
                    Name = o.Name
                });

            var dbChannels = (await dbChannelsQuery.ToListAsync())
                .Where(o => !channels.Any(x => x.Id == o.Id));

            channels.AddRange(dbChannels);

            var result = channels
                .OrderBy(o => o.Name)
                .ToDictionary(o => o.Id, o => o.Name);
            return Ok(result);
        }

        /// <summary>
        /// Get roles of guild.
        /// </summary>
        [HttpGet("{guildId}/roles")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetRolesOfGuild))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<Dictionary<string, string>> GetRolesOfGuild(ulong guildId)
        {
            var guild = DiscordClient.GetGuild(guildId);
            if (guild == null)
                return Ok(new Dictionary<string, string>());

            var roles = guild.Roles
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
