using GrillBot.App.Actions.Api.V3.DataResolve;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[JwtAuthorize]
[ApiExplorerSettings(GroupName = "v3")]
public class LookupController : Core.Infrastructure.Actions.ControllerBase
{
    public LookupController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpGet("guild/{guildId}")]
    [ProducesResponseType(typeof(Guild), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveGuildAsync(ulong guildId)
        => ProcessAsync<LookupAction>(DataResolveType.Guild, guildId);

    [HttpGet("channel/{guildId}/{channelId}")]
    [ProducesResponseType(typeof(Channel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveChannelAsync(ulong guildId, ulong channelId)
        => ProcessAsync<LookupAction>(DataResolveType.Channel, guildId, channelId);

    [HttpGet("role/{roleId}")]
    [ProducesResponseType(typeof(Role), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveRoleAsync(ulong roleId)
        => ProcessAsync<LookupAction>(DataResolveType.Role, roleId);

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveUserAsync(ulong userId)
        => ProcessAsync<LookupAction>(DataResolveType.User, userId);

    [HttpGet("user/{guildId}/{userId}")]
    [ProducesResponseType(typeof(GuildUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveGuildUserAsync(ulong guildId, ulong userId)
        => ProcessAsync<LookupAction>(DataResolveType.GuildUser, guildId, userId);
}
