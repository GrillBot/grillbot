using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.App.Services;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/invite")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class InviteController : Controller
{
    private InviteService InviteService { get; }
    private ApiRequestContext Context { get; }
    private IServiceProvider ServiceProvider { get; }

    public InviteController(InviteService inviteService, ApiRequestContext context, IServiceProvider serviceProvider)
    {
        InviteService = inviteService;
        Context = context;
        ServiceProvider = serviceProvider;
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
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Invite.GetInviteList>();
        var result = await action.ProcessAsync(parameters);
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
        var result = await InviteService.RefreshMetadataAsync(Context.LoggedUser, true);
        return Ok(result);
    }

    /// <summary>
    /// Get count of items in metadata cache.
    /// </summary>
    /// <response code="200">Returns count of current items in cache.</response>
    [HttpGet("metadata/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetCurrentMetadataCountAsync()
    {
        var result = await InviteService.GetMetadataCountAsync();
        return Ok(result);
    }
}
