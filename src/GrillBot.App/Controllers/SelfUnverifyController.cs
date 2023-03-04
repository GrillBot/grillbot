using GrillBot.App.Actions;
using GrillBot.Common.Infrastructure;
using GrillBot.Data.Models.API.Selfunverify;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/selfunverify/keep")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class SelfUnverifyController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public SelfUnverifyController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get non paginated list of keepable roles and channels.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetKeepablesListAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Unverify.GetKeepablesList>();
        var result = await action.ProcessAsync(null);

        return Ok(result);
    }

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
        ApiAction.Init(this, parameters.OfType<IApiObject>().ToArray());

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Unverify.AddKeepables>();
        await action.ProcessAsync(parameters);
        return Ok();
    }

    /// <summary>
    /// Check if keepable exists. For validation purposes.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("exist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> KeepableExistsAsync([FromQuery] KeepableParams parameters)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Unverify.KeepableExists>();
        var result = await action.ProcessAsync(parameters);

        return Ok(result);
    }

    /// <summary>
    /// Remove keepable item or group.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> KeepableRemoveAsync(string group, string name = null)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Unverify.RemoveKeepables>();
        await action.ProcessAsync(group, name);

        return Ok();
    }
}
