using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class PermissionsController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public PermissionsController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates explicit permission.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">Permission exists.</response>
    [HttpPost("explicit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.Conflict)]
    public async Task<ActionResult> CreateExplicitPermissionAsync([FromBody] CreateExplicitPermissionParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.CreateExplicitPermission>();
        await action.ProcessAsync(parameters);

        if (action.IsConflict)
            return Conflict(new MessageResponse(action.ErrorMessage));
        return Ok();
    }

    /// <summary>
    /// Removes explicit permission
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    /// <response code="404">Permission not found</response>
    [HttpDelete("explicit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> RemoveExplicitPermissionAsync([Required] string command, [Required] string targetId)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.RemoveExplicitPermission>();
        await action.ProcessAsync(command, targetId);

        return Ok();
    }

    /// <summary>
    /// Gets non-paginated list of explicit permissions.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("explicit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<ExplicitPermission>>> GetExplicitPermissionsListAsync([FromQuery] string searchQuery)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.GetExplicitPermissionList>();
        var result = await action.ProcessAsync(searchQuery);

        return Ok(result);
    }

    /// <summary>
    /// Sets permission state of command.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">Permission not found.</response>
    [HttpPut("set")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> SetExplicitPermissionStateAsync([Required] string command, [Required] string targetId, ExplicitPermissionState state)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.SetExplicitPermissionState>();
        await action.ProcessAsync(command, targetId, state);
        return Ok();
    }
}
