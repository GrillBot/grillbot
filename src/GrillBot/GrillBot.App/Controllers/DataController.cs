using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrillBot.App.Services.Emotes;
using GrillBot.Data.Models.API.Emotes;
using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/data")]
[ApiExplorerSettings(GroupName = "v1")]
public class DataController : Controller
{
    private IDiscordClient DiscordClient { get; }
    private EmotesCacheService EmotesCacheService { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ApiRequestContext ApiRequestContext { get; }
    private IServiceProvider ServiceProvider { get; }

    public DataController(IDiscordClient discordClient, EmotesCacheService emotesCacheService, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder, ApiRequestContext apiRequestContext,
        IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        EmotesCacheService = emotesCacheService;
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
        ApiRequestContext = apiRequestContext;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get non paginated list of available guilds.
    /// </summary>
    [HttpGet("guilds")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableGuildsAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetAvailableGuilds>();
        var result = await action.ProcessAsync();

        return Ok(result);
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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.GetChannelSimpleList>();
        var result = await action.ProcessAsync(guildId, ignoreThreads);

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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetRoles>();
        var result = await action.ProcessAsync(guildId);

        return Ok(result);
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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.GetCommandsList>();
        var result = action.Process();

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
