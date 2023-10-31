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
public class SelfUnverifyController : Core.Infrastructure.Actions.ControllerBase
{
    public SelfUnverifyController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get non paginated list of keepable roles and channels.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(typeof(Dictionary<string, List<string>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKeepablesListAsync()
        => await ProcessAsync<GetKeepablesList>((string?)null);

    /// <summary>
    /// Add new keepable role or channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddKeepableAsync([FromBody] List<KeepableParams> parameters)
    {
        ApiAction.Init(this, parameters.OfType<IDictionaryObject>().ToArray());
        return await ProcessAsync<AddKeepables>(parameters);
    }

    /// <summary>
    /// Check if keepable exists. For validation purposes.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("exist")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> KeepableExistsAsync([FromQuery] KeepableParams parameters)
        => await ProcessAsync<KeepableExists>(parameters);

    /// <summary>
    /// Remove keepable item or group.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> KeepableRemoveAsync(string group, string? name = null)
        => await ProcessAsync<RemoveKeepables>(group, name);
}
