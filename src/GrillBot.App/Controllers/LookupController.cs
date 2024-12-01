using GrillBot.App.Actions.Api.V3.Lookup;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[JwtAuthorize("Lookups")]
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

    [HttpGet("guild/list")]
    [ProducesResponseType(typeof(List<Guild>), StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "LookupListCache")]
    public Task<IActionResult> ResolveGuildListAsync()
        => ProcessAsync<LookupListAction>(DataResolveType.Guild);

    [HttpGet("channel/{channelId}")]
    [ProducesResponseType(typeof(Channel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveChannelAsync(ulong channelId)
        => ProcessAsync<LookupAction>(DataResolveType.Channel, channelId);

    [HttpGet("channel/list")]
    [ProducesResponseType(typeof(List<Channel>), StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "LookupListCache")]
    public Task<IActionResult> ResolveChannelListAsync()
        => ProcessAsync<LookupListAction>(DataResolveType.Channel);

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

    [HttpGet("user/list")]
    [ProducesResponseType(typeof(List<User>), StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "LookupListCache")]
    public Task<IActionResult> ResolveUserListAsync()
        => ProcessAsync<LookupListAction>(DataResolveType.User);

    [HttpGet("user/{guildId}/{userId}")]
    [ProducesResponseType(typeof(GuildUser), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveGuildUserAsync(ulong guildId, ulong userId)
        => ProcessAsync<LookupAction>(DataResolveType.GuildUser, guildId, userId);

    [HttpGet("sas")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ResolveSasLinkAsync([Required, FromQuery] string filename)
        => ProcessAsync<LookupAction>(DataResolveType.FileSasLink, filename);
}
