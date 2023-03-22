using GrillBot.App.Actions.Api.V1.Channel;
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
public class DataController : Infrastructure.ControllerBase
{
    public DataController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get non paginated list of available guilds.
    /// </summary>
    [HttpGet("guilds")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableGuildsAsync()
        => Ok(await ProcessActionAsync<GetAvailableGuilds, Dictionary<string, string>>(action => action.ProcessAsync()));

    /// <summary>
    /// Get non paginated list of channels.
    /// </summary>
    /// <param name="guildId">Optional guild ID</param>
    /// <param name="ignoreThreads">Flag that removes threads from list.</param>
    [HttpGet("channels")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetChannelsAsync(ulong? guildId, bool ignoreThreads = false)
        => Ok(await ProcessActionAsync<GetChannelSimpleList, Dictionary<string, string>>(action => action.ProcessAsync(guildId, ignoreThreads)));

    /// <summary>
    /// Get roles
    /// </summary>
    [HttpGet("roles")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetRolesAsync(ulong? guildId)
        => Ok(await ProcessActionAsync<GetRoles, Dictionary<string, string>>(action => action.ProcessAsync(guildId)));

    /// <summary>
    /// Gets non-paginated list of users.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("users")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<Dictionary<string, string>>> GetAvailableUsersAsync(bool? bots = null, ulong? guildId = null)
        => Ok(await ProcessActionAsync<GetAvailableUsers, Dictionary<string, string>>(action => action.ProcessAsync(bots, guildId)));

    /// <summary>
    /// Get currently supported emotes.
    /// </summary>
    [HttpGet("emotes")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<EmoteItem>>> GetSupportedEmotes()
        => Ok(await ProcessActionAsync<GetSupportedEmotes, List<EmoteItem>>(action => action.ProcessAsync()));

    /// <summary>
    /// Get list of methods available from public api.
    /// </summary>
    /// <response code="200">Returns list of methods available from public api.</response>
    [HttpGet("publicApi/methods")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<List<string>> GetPublicApiMethods()
        => Ok(ProcessAction<GetPublicApiMethods, List<string>>(action => action.Process()));
}
