using GrillBot.App.Services;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Invites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/invite")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[OpenApiTag("Invites", Description = "Invite management")]
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
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet]
    [OpenApiOperation(nameof(InviteController) + "_" + nameof(GetInviteListAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<GuildInvite>>> GetInviteListAsync([FromQuery] GetInviteListParams parameters, CancellationToken cancellationToken)
    {
        var result = await InviteService.GetInviteListAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refresh invite metadata cache.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPost("metadata/refresh")]
    [OpenApiOperation(nameof(InviteController) + "_" + nameof(RefreshMetadataCacheAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, int>>> RefreshMetadataCacheAsync()
    {
        var result = await InviteService.RefreshMetadataAsync();
        return Ok(result);
    }

    /// <summary>
    /// Get count of items in metadata cache.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("metadata/count")]
    [OpenApiOperation(nameof(InviteController) + "_" + nameof(GetCurrentMetadataCount))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<int> GetCurrentMetadataCount()
    {
        var result = InviteService.GetMetadataCount();
        return Ok(result);
    }
}
