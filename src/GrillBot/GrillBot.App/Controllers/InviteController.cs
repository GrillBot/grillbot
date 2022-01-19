using GrillBot.App.Services;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Invites;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/invite")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [OpenApiTag("Invites", Description = "Invite management")]
    public class InviteController : Controller
    {
        private GrillBotContext DbContext { get; }
        private InviteService InviteService { get; }

        public InviteController(GrillBotContext dbContext, InviteService inviteService)
        {
            DbContext = dbContext;
            InviteService = inviteService;
        }

        /// <summary>
        /// Gets pagniated list of invites.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        [HttpGet]
        [OpenApiOperation(nameof(InviteController) + "_" + nameof(GetInviteListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<PaginatedResponse<GuildInvite>>> GetInviteListAsync([FromQuery] GetInviteListParams parameters)
        {
            var query = DbContext.Invites.AsNoTracking()
                .Include(o => o.Creator).ThenInclude(o => o.User)
                .Include(o => o.UsedUsers)
                .Include(o => o.Guild)
                .AsQueryable();

            query = parameters.CreateQuery(query);
            var result = await PaginatedResponse<GuildInvite>.CreateAsync(query, parameters, entity => new(entity));
            return Ok(result);
        }
    }
}
