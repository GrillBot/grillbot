using GrillBot.App.Services.Unverify;
using GrillBot.Data.Models.API.Selfunverify;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/selfunverify")]
[OpenApiTag("SelfUnverify", Description = "SelfUnverify management")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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
    [HttpGet("keep")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Dictionary<string, List<string>>>> GetKeepablesListAsync(CancellationToken cancellationToken)
    {
        var result = await SelfunverifyService.GetKeepablesAsync(null, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Add new role or channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("keep")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddKeepableAsync([FromBody] List<KeepableParams> parameters, CancellationToken cancellationToken)
    {
        try
        {
            await SelfunverifyService.AddKeepablesAsync(parameters, cancellationToken);
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
    [HttpGet("keep/exist")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> KeepableExistsAsync([FromQuery] KeepableParams parameters, CancellationToken cancellationToken)
    {
        var result = await SelfunverifyService.KeepableExistsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Remove keepable item or group.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpDelete("keep")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> KeepableRemoveAsync(string group, string name = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await SelfunverifyService.RemoveKeepableAsync(group, name, cancellationToken);
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
