using GrillBot.App.Services;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/invite")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class InviteController : Controller
{
    private InviteService InviteService { get; }

    public InviteController(InviteService inviteService)
    {
        InviteService = inviteService;
    }

    /// <summary>
    /// Get pagniated list of invites.
    /// </summary>
    /// <response code="200">Returns paginated list of created and used invites.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<GuildInvite>>> GetInviteListAsync([FromBody] GetInviteListParams parameters)
    {
        this.StoreParameters(parameters);

        var result = await InviteService.GetInviteListAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Refresh invite metadata cache.
    /// </summary>
    /// <response code="200">Returns report per server.</response>
    [HttpPost("metadata/refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> RefreshMetadataCacheAsync()
    {
        var result = await InviteService.RefreshMetadataAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get count of items in metadata cache.
    /// </summary>
    /// <response code="200">Returns count of current items in cache.</response>
    [HttpGet("metadata/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<int> GetCurrentMetadataCount()
    {
        var result = InviteService.GetMetadataCount();
        return Ok(result);
    }
}
