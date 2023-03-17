using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Core.Infrastructure;
using GrillBot.Data.Models.API.Selfunverify;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/selfunverify/keep")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class SelfUnverifyController : Infrastructure.ControllerBase
{
    public SelfUnverifyController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get non paginated list of keepable roles and channels.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetKeepablesListAsync()
        => Ok(await ProcessActionAsync<GetKeepablesList, Dictionary<string, List<string>>>(action => action.ProcessAsync(null)));

    /// <summary>
    /// Add new keepable role or channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddKeepableAsync([FromBody] List<KeepableParams> parameters)
    {
        ApiAction.Init(this, parameters.OfType<IDictionaryObject>().ToArray());

        await ProcessActionAsync<AddKeepables>(action => action.ProcessAsync(parameters));
        return Ok();
    }

    /// <summary>
    /// Check if keepable exists. For validation purposes.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("exist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> KeepableExistsAsync([FromQuery] KeepableParams parameters)
        => Ok(await ProcessActionAsync<KeepableExists, bool>(action => action.ProcessAsync(parameters)));

    /// <summary>
    /// Remove keepable item or group.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> KeepableRemoveAsync(string group, string? name = null)
    {
        await ProcessActionAsync<RemoveKeepables>(action => action.ProcessAsync(group, name));
        return Ok();
    }
}
