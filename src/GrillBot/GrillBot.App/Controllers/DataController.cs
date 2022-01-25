using Discord.Commands;
using Discord.Interactions;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Database.Enums;
using GrillBot.Database.Entity;

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
        private IConfiguration Configuration { get; }
        private InteractionService InteractionService { get; }

        public DataController(DiscordSocketClient discordClient, GrillBotContext dbContext, CommandService commandService,
            IConfiguration configuration, InteractionService interactionService)
        {
            DiscordClient = discordClient;
            DbContext = dbContext;
            CommandService = commandService;
            Configuration = configuration;
            InteractionService = interactionService;
        }

        /// <summary>
        /// Get non paginated list of available guilds.
        /// </summary>
        [HttpGet("guilds")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetAvailableGuildsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetAvailableGuildsAsync()
        {
            var guildsQuery = DbContext.Guilds.AsNoTracking();

            if (User.HaveUserPermission())
            {
                var currentUserId = User.GetUserId();
                var mutualGuilds = DiscordClient.FindMutualGuilds(currentUserId)
                    .Select(o => o.Id.ToString()).ToList();

                guildsQuery = guildsQuery.Where(o => mutualGuilds.Contains(o.Id));
            }

            var guilds = await guildsQuery
                .Select(o => new { o.Id, o.Name })
                .OrderBy(o => o.Name)
                .ToDictionaryAsync(o => o.Id, o => o.Name);

            return Ok(guilds);
        }

        /// <summary>
        /// Get non paginated list of channels.
        /// </summary>
        /// <param name="guildId">Optional guild ID</param>
        /// <param name="ignoreThreads">Flag that removes threads from list.</param>
        [HttpGet("channels")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetChannelsAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetChannelsAsync(ulong? guildId, bool ignoreThreads = false)
        {
            var currentUserId = User.GetUserId();
            IEnumerable<SocketGuild> guilds;
            if (User.HaveUserPermission())
                guilds = DiscordClient.FindMutualGuilds(currentUserId);
            else
                guilds = DiscordClient.Guilds.AsEnumerable();
            if (guildId != null) guilds = guilds.Where(o => o.Id == guildId.Value);

            var availableChannels = User.HaveUserPermission() ?
                guilds.SelectMany(o => o.GetAvailableChannelsFor(o.GetUser(currentUserId))).ToList() :
                guilds.SelectMany(o => o.Channels);

            var channels = availableChannels.Select(o => new Channel(o))
                .Where(o => o.Type != null && o.Type != ChannelType.Category)
                .ToList();

            if (ignoreThreads)
                channels = channels.FindAll(o => o.Type != ChannelType.PrivateThread && o.Type != ChannelType.PublicThread);

            var guildIds = guilds.Select(o => o.Id.ToString()).ToList();
            var dbChannelsQuery = DbContext.Channels.AsNoTracking()
                .Where(o => o.ChannelType != ChannelType.Category && guildIds.Contains(o.GuildId))
                .OrderBy(o => o.Name)
                .AsQueryable();

            if (ignoreThreads)
                dbChannelsQuery = dbChannelsQuery.Where(o => o.ChannelType != ChannelType.PublicThread && o.ChannelType != ChannelType.PrivateThread);

            var query = dbChannelsQuery.Select(o => new Channel()
            {
                Type = o.ChannelType,
                Id = o.ChannelId,
                Name = o.Name
            });

            var dbChannels = (await query.ToListAsync())
                .Where(o => !channels.Any(x => x.Id == o.Id));

            channels.AddRange(dbChannels);

            var result = channels
                .OrderBy(o => o.Name)
                .ToDictionary(o => o.Id, o => $"{o.Name} {(o.Type == ChannelType.PublicThread || o.Type == ChannelType.PrivateThread ? "(Thread)" : "")}".Trim());

            return Ok(result);
        }

        /// <summary>
        /// Get roles
        /// </summary>
        [HttpGet("roles")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetRoles))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<Dictionary<string, string>> GetRoles(ulong? guildId)
        {
            var currentUserId = User.GetUserId();
            IEnumerable<SocketGuild> guilds;
            if (User.HaveUserPermission())
                guilds = DiscordClient.FindMutualGuilds(currentUserId);
            else
                guilds = DiscordClient.Guilds.AsEnumerable();
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
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetCommandsList))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<string>> GetCommandsList()
        {
            var commands = CommandService.Commands
                .Select(o => Configuration.GetValue<string>("Discord:Commands:Prefix") + (o.Aliases[0]?.Trim()))
                .Distinct();

            var slashCommands = InteractionService.SlashCommands
                .Select(o => o.ToString().Trim())
                .Where(o => !string.IsNullOrEmpty(o))
                .Select(o => $"/{o}")
                .Distinct();

            var result = commands.Concat(slashCommands).OrderBy(o => o[1..]).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Gets non-paginated list of users.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("users")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
        [OpenApiOperation(nameof(DataController) + "_" + nameof(GetAvailableUsersAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<Dictionary<string, string>>> GetAvailableUsersAsync(bool? bots = null)
        {
            var query = DbContext.Users.AsNoTracking().AsQueryable();

            if (bots != null)
            {
                if (bots == true)
                    query = query.Where(o => (o.Flags & (int)UserFlags.NotUser) != 0);
                else
                    query = query.Where(o => (o.Flags & (int)UserFlags.NotUser) == 0);
            }

            if (User.HaveUserPermission())
            {
                var currentUserId = User.GetUserId();
                var mutualGuilds = DiscordClient.FindMutualGuilds(currentUserId)
                    .Select(o => o.Id.ToString()).ToList();

                query = query.Where(o => o.Guilds.Any(x => mutualGuilds.Contains(x.GuildId)));
            }

            query = query.Select(o => new User() { Id = o.Id, Username = o.Username })
                .OrderBy(o => o.Username);

            var dict = await query.ToDictionaryAsync(o => o.Id, o => o.Username);
            return Ok(dict);
        }
    }
}
