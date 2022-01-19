using GrillBot.App.Services;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [OpenApiTag("Authentication", Description = "OAuth2 discord authentication")]
    public class AuthController : Controller
    {
        private OAuth2Service Service { get; }

        public AuthController(OAuth2Service service)
        {
            Service = service;
        }

        /// <summary>
        /// Gets redirect uri to access OAuth2.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("link")]
        [AllowAnonymous]
        [OpenApiOperation(nameof(AuthController) + "_" + nameof(GetRedirectLink))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<OAuth2GetLink> GetRedirectLink(bool isPublic)
        {
            var link = Service.GetRedirectLink(isPublic);
            return Ok(link);
        }

        /// <summary>
        /// OAuth2 redirect callback
        /// </summary>
        /// <param name="code">Authorization code</param>
        /// <param name="state">Public or private administration</param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("callback")]
        [AllowAnonymous]
        [OpenApiOperation(nameof(AuthController) + "_" + nameof(OnOAuth2CallbackAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> OnOAuth2CallbackAsync([FromQuery, Required] string code, [Required, FromQuery] bool state)
        {
            var redirectUrl = await Service.CreateRedirectUrlAsync(code, state);
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Creates auth token from session.
        /// </summary>
        /// <param name="sessionId">SessionId</param>
        /// <param name="isPublic">Public or private administration</param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("token")]
        [AllowAnonymous]
        [OpenApiOperation(nameof(AuthController) + "_" + nameof(CreateLoginTokenAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<OAuth2LoginToken>> CreateLoginTokenAsync([FromQuery, Required] string sessionId, [FromQuery, Required] bool isPublic)
        {
            var token = await Service.CreateTokenAsync(sessionId, isPublic);
            return Ok(token);
        }
    }
}
