using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auth")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class AuthController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public AuthController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get redirect link to access OAuth2 gateway.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("link")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<OAuth2GetLink> GetRedirectLink([FromQuery] AuthState state)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Auth.GetRedirectLink>();
        var result = action.Process(state);

        return Ok(result);
    }

    /// <summary>
    /// OAuth2 redirect callback.
    /// </summary>
    /// <param name="code">Authorization code</param>
    /// <param name="state">Public or private administration</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet("callback")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> OnOAuth2CallbackAsync([FromQuery, Required] string code, [Required, FromQuery] string state)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Auth.ProcessCallback>();
        var redirectUrl = await action.ProcessAsync(code, state);
        return Redirect(redirectUrl);
    }

    /// <summary>
    /// Create auth token from session.
    /// </summary>
    /// <param name="sessionId">SessionId</param>
    /// <param name="isPublic">Public or private administration</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet("token")]
    [AllowAnonymous]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<OAuth2LoginToken>> CreateLoginTokenAsync([FromQuery, Required] string sessionId, [FromQuery, Required] bool isPublic)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Auth.CreateToken>();
        var token = await action.ProcessAsync(sessionId, isPublic);
        return Ok(token);
    }

    /// <summary>
    /// Create auth token from user ID. Only for development purposes.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isPublic">Public or private administration.</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed or user not found.</response>
    [HttpGet("token/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [OnlyDevelopment]
    public async Task<ActionResult<OAuth2LoginToken>> CreateLoginTokenFromIdAsync(ulong userId, [FromQuery] bool isPublic = false)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Auth.CreateToken>();
        var token = await action.ProcessAsync(userId, isPublic);
        return Ok(token);
    }
}
