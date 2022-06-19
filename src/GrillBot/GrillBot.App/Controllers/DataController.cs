using Discord.Commands;
using Discord.Interactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using GrillBot.Data.Models.API.Channels;
using GrillBot.App.Infrastructure.Preconditions.TextBased;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Guilds;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/data")]
[OpenApiTag("Data", Description = "Support for form fields, ...")]
[ResponseCache(CacheProfileName = "ConstsApi")]
public class DataController : Controller
{
    private DiscordSocketClient DiscordClient { get; }
    private CommandService CommandService { get; }
    private IConfiguration Configuration { get; }
    private InteractionService InteractionService { get; }
    private EmotesCacheService EmotesCacheService { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ApiRequestContext ApiRequestContext { get; }

    public DataController(DiscordSocketClient discordClient, CommandService commandService, IConfiguration configuration,
        InteractionService interactionService, EmotesCacheService emotesCacheService, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder, ApiRequestContext apiRequestContext)
    {
        DiscordClient = discordClient;
        CommandService = commandService;
        Configuration = configuration;
        InteractionService = interactionService;
        EmotesCacheService = emotesCacheService;
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
        ApiRequestContext = apiRequestContext;
    }

    /// <summary>
    /// Get non paginated list of available guilds.
    /// </summary>
    [HttpGet("guilds")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableGuildsAsync()
    {
        var guildsFilter = new GetGuildListParams
        {
            Pagination = { Page = 1, PageSize = int.MaxValue },
        };

        if (ApiRequestContext.IsPublic())
        {
            var loggedUserId = ApiRequestContext.GetUserId();
            var mutualGuilds = await DiscordClient.FindMutualGuildsAsync(loggedUserId);

            guildsFilter.MutualGuildIds.AddRange(mutualGuilds.Select(o => o.Id.ToString()));
        }

        await using var repository = DatabaseBuilder.CreateRepository();

        var guildsData = await repository.Guild.GetGuildListAsync(guildsFilter, guildsFilter.Pagination);
        var guilds = guildsData.Data
            .ToDictionary(o => o.Id, o => o.Name);

        return Ok(guilds);
    }

    /// <summary>
    /// Get non paginated list of channels.
    /// </summary>
    /// <param name="guildId">Optional guild ID</param>
    /// <param name="ignoreThreads">Flag that removes threads from list.</param>
    [HttpGet("channels")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetChannelsAsync(ulong? guildId, bool ignoreThreads = false)
    {
        var loggedUserId = ApiRequestContext.GetUserId();
        var availableGuilds = ApiRequestContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(loggedUserId) : DiscordClient.Guilds.OfType<IGuild>().ToList();

        if (guildId != null)
            availableGuilds = availableGuilds.FindAll(o => o.Id == guildId.Value);

        var availableChannels = new List<IGuildChannel>();
        foreach (var guild in availableGuilds)
        {
            if (ApiRequestContext.IsPublic())
            {
                var guildUser = await guild.GetUserAsync(loggedUserId);
                availableChannels.AddRange(await guild.GetAvailableChannelsAsync(guildUser, !ignoreThreads));
            }
            else
            {
                // Get all channels (if wanted ignore threads, ignore it). 
                availableChannels.AddRange((await guild.GetChannelsAsync()).Where(o => !ignoreThreads || o is not IThreadChannel));
            }
        }

        var channels = availableChannels
            .Select(o => Mapper.Map<Channel>(o))
            .Where(o => o.Type != null && o.Type != ChannelType.Category)
            .ToList();

        var guildIds = availableChannels.Select(o => o.Id.ToString()).ToList();
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbChannels = await repository.Channel.GetAllChannelsAsync(guildIds, ignoreThreads, true);
        dbChannels = dbChannels.FindAll(o => channels.All(x => x.Id != o.ChannelId)); // Select from DB all channels that is not visible.
        channels.AddRange(Mapper.Map<List<Channel>>(dbChannels));

        var result = channels
            .DistinctBy(o => o.Id)
            .OrderBy(o => o.Name)
            .ToDictionary(o => o.Id, o => $"{o.Name} {(o.Type is ChannelType.PublicThread or ChannelType.PrivateThread or ChannelType.NewsThread ? "(Thread)" : "")}".Trim());

        return Ok(result);
    }

    /// <summary>
    /// Get roles
    /// </summary>
    [HttpGet("roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetRolesAsync(ulong? guildId)
    {
        var loggedUserId = ApiRequestContext.GetUserId();

        var availableGuilds = ApiRequestContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(loggedUserId) : DiscordClient.Guilds.OfType<IGuild>().ToList();
        if (guildId != null)
            availableGuilds = availableGuilds.FindAll(o => o.Id == guildId.Value);

        var roles = availableGuilds
            .Select(o => o.Roles.Where(x => x.Id != o.EveryoneRole.Id))
            .SelectMany(o => o)
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
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<List<string>> GetCommandsList()
    {
        var commands = CommandService.Modules
            .Where(o => o.Commands.Count > 0 && !o.Preconditions.OfType<TextCommandDeprecatedAttribute>().Any())
            .Select(o => o.Commands.Where(x => !x.Preconditions.OfType<TextCommandDeprecatedAttribute>().Any()))
            .SelectMany(o => o.Select(x => Configuration.GetValue<string>("Discord:Commands:Prefix") + (x.Aliases[0].Trim())).Distinct())
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
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableUsersAsync(bool? bots = null)
    {
        var loggedUserId = ApiRequestContext.GetUserId();
        var mutualGuilds = ApiRequestContext.IsPublic() ? await DiscordClient.FindMutualGuildsAsync(loggedUserId) : null;
        var mutualGuildIds = mutualGuilds?.ConvertAll(o => o.Id.ToString());

        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.User.GetFullListOfUsers(bots, mutualGuildIds);
        var result = data.ToDictionary(o => o.Id, o => $"{o.Username}#{o.Discriminator}");

        return Ok(result);
    }

    /// <summary>
    /// Get currently supported emotes.
    /// </summary>
    [HttpGet("emotes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<EmoteItem>> GetSupportedEmotes()
    {
        var emotes = EmotesCacheService.GetSupportedEmotes();

        var result = Mapper.Map<List<EmoteItem>>(emotes)
            .OrderBy(o => o.Name)
            .ToList();

        return Ok(result);
    }
}
