using GrillBot.App.Actions.Api.V1.Channel.SimpleList;
using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.App.Actions.Api.V1.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Data.Models.API.Emotes;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/data")]
[ApiExplorerSettings(GroupName = "v1")]
public class DataController : Core.Infrastructure.Actions.ControllerBase
{
    public DataController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get non paginated list of available guilds.
    /// </summary>
    [HttpGet("guilds")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableGuildsAsync()
        => await ProcessAsync<GetAvailableGuilds>();

    /// <summary>
    /// Get non paginated list of channels.
    /// </summary>
    /// <param name="guildId">Optional guild ID</param>
    /// <param name="ignoreThreads">Flag that removes threads from list.</param>
    [HttpGet("channels")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChannelsAsync(ulong? guildId, bool ignoreThreads = false)
        => await ProcessAsync<GetChannelSimpleList>(ignoreThreads, guildId);

    /// <summary>
    /// Get non paginated list of channels that contains some pin.
    /// </summary>
    /// <response code="200">Returns simple list of channels that contains some pin.</response>
    [HttpGet("channels/pins")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChannelsWithPinsAsync()
        => await ProcessAsync<GetChannelSimpleListWithPins>();

    /// <summary>
    /// Get roles
    /// </summary>
    [HttpGet("roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesAsync(ulong? guildId)
        => await ProcessAsync<GetRoles>(guildId);

    /// <summary>
    /// Gets non-paginated list of users.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(typeof(Dictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableUsersAsync(bool? bots = null, ulong? guildId = null)
        => await ProcessAsync<GetAvailableUsers>(bots, guildId);

    /// <summary>
    /// Get currently supported emotes.
    /// </summary>
    [HttpGet("emotes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(List<GuildEmoteItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedEmotesAsync()
        => await ProcessAsync<GetSupportedEmotes>();

    /// <summary>
    /// Get list of methods available from public api.
    /// </summary>
    /// <response code="200">Returns list of methods available from public api.</response>
    [HttpGet("publicApi/methods")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicApiMethodsAsync()
        => await ProcessAsync<GetPublicApiMethods>();
}
