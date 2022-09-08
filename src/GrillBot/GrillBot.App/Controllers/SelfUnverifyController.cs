using GrillBot.App.Services.Unverify;
using GrillBot.Common.Infrastructure;
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
public class SelfUnverifyController : Controller
{
    private SelfunverifyService SelfunverifyService { get; }

    public SelfUnverifyController(SelfunverifyService selfunverifyService)
    {
        SelfunverifyService = selfunverifyService;
    }

    /// <summary>
    /// Get non paginated list of keepable roles and channels.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetKeepablesListAsync()
    {
        var result = await SelfunverifyService.GetKeepablesAsync(null);
        return Ok(result);
    }

    /// <summary>
    /// Add new role or channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddKeepableAsync([FromBody] List<KeepableParams> parameters)
    {
        try
        {
            this.StoreParameters(parameters.OfType<IApiObject>().ToArray());
            await SelfunverifyService.AddKeepablesAsync(parameters);
            return Ok();
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("Exist", ex.Message);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return BadRequest(problemDetails);
        }
    }

    /// <summary>
    /// Check if keepable exists. For validation purposes.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("exist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> KeepableExistsAsync([FromQuery] KeepableParams parameters)
    {
        var result = await SelfunverifyService.KeepableExistsAsync(parameters);
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
        try
        {
            await SelfunverifyService.RemoveKeepableAsync(group, name);
            return Ok();
        }
        catch (ValidationException ex)
        {
            ModelState.AddModelError("Exist", ex.Message);
            var problemDetails = new ValidationProblemDetails(ModelState);
            return BadRequest(problemDetails);
        }
    }
}
