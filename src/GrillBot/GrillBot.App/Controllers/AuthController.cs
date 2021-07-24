using GrillBot.App.Services;
using GrillBot.Data.Models.API.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

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
        public ActionResult<OAuth2GetLink> GetRedirectLink()
        {
            var link = Service.GetRedirectLink();
            return Ok(link);
        }

        /// <summary>
        /// OAuth2 redirect callback
        /// </summary>
        /// <param name="code">Authorization code</param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("callback")]
        [AllowAnonymous]
        [OpenApiOperation(nameof(AuthController) + "_" + nameof(OnOAuth2CallbackAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> OnOAuth2CallbackAsync([FromQuery, Required] string code)
        {
            var redirectUrl = await Service.CreateRedirectUrlAsync(code);
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Creates auth token from session.
        /// </summary>
        /// <param name="sessionId">SessionId</param>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet("token")]
        [AllowAnonymous]
        [OpenApiOperation(nameof(AuthController) + "_" + nameof(CreateLoginTokenAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<OAuth2LoginToken>> CreateLoginTokenAsync([FromQuery, Required] string sessionId)
        {
            var token = await Service.CreateTokenAsync(sessionId);
            return Ok(token);
        }
    }
}
