using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Invite;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Invites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/invite")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class InviteController : Core.Infrastructure.Actions.ControllerBase
{
    public InviteController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get pagniated list of invites.
    /// </summary>
    /// <response code="200">Returns paginated list of created and used invites.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType(typeof(PaginatedResponse<GuildInvite>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInviteListAsync([FromBody] GetInviteListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetInviteList>(parameters);
    }

    /// <summary>
    /// Refresh invite metadata cache.
    /// </summary>
    /// <response code="200">Returns report per server.</response>
    [HttpPost("metadata/refresh")]
    [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshMetadataCacheAsync()
        => await ProcessAsync<RefreshMetadata>(true);

    /// <summary>
    /// Get count of items in metadata cache.
    /// </summary>
    /// <response code="200">Returns count of current items in cache.</response>
    [HttpGet("metadata/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentMetadataCountAsync()
        => await ProcessAsync<GetMetadataCount>();

    /// <summary>
    /// Delete invite.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">Unable to find invite.</response>
    [HttpDelete("{guildId}/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInviteAsync(ulong guildId, string code)
        => await ProcessAsync<DeleteInvite>(guildId, code);
}
