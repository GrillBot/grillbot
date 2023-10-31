using GrillBot.App.Actions.Api.V1.Auth;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auth")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuthController : Core.Infrastructure.Actions.ControllerBase
{
    public AuthController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get redirect link to access OAuth2 gateway.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("link")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuth2GetLink), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRedirectLinkAsync([FromQuery] AuthState state)
        => await ProcessAsync<GetRedirectLink>(state);

    /// <summary>
    /// OAuth2 redirect callback.
    /// </summary>
    /// <param name="code">Authorization code</param>
    /// <param name="state">Public or private administration</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet("callback")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OnOAuth2CallbackAsync([FromQuery, Required] string code, [Required, FromQuery] string state)
        => await ProcessAsync<ProcessCallback>(code, state);

    /// <summary>
    /// Create auth token from session.
    /// </summary>
    /// <param name="sessionId">SessionId</param>
    /// <param name="isPublic">Public or private administration</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet("token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuth2LoginToken), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateLoginTokenAsync([FromQuery, Required] string sessionId, [FromQuery, Required] bool isPublic)
        => await ProcessAsync<CreateToken>(sessionId, isPublic);

    /// <summary>
    /// Create auth token from user ID. Only for development purposes.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isPublic">Public or private administration.</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed or user not found.</response>
    [HttpGet("token/{userId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(OAuth2LoginToken), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [OnlyDevelopment]
    public async Task<IActionResult> CreateLoginTokenFromIdAsync(ulong userId, [FromQuery] bool isPublic = false)
        => await ProcessAsync<CreateToken>(userId, isPublic);
}
