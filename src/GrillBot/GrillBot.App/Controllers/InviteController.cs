using Discord.WebSocket;
using GrillBot.App.Services;
using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Invites;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/invite")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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

        /// <summary>
        /// Gets non-paginated list of active invites.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("active")]
        [OpenApiOperation(nameof(InviteController) + "_" + nameof(GetActiveInviteListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<GuildInvite>>> GetActiveInviteListAsync()
        {
            var guilds = await DbContext.Guilds.AsNoTracking()
                .Select(o => new Database.Entity.Guild() { Id = o.Id, Name = o.Name })
                .ToListAsync();

            var users = await DbContext.Users.AsNoTracking()
                .Where(o => (o.Flags & (int)UserFlags.NotUser) == 0)
                .ToListAsync();

            var invites = InviteService.MetadataCache.Select(o =>
            {
                var creator = users.Find(x => x.Id == o.CreatorId?.ToString());
                var guild = guilds.Find(x => x.Id == o.GuildId.ToString());

                return new GuildInvite()
                {
                    Code = o.Code,
                    CreatedAt = o.CreatedAt,
                    UsedUsersCount = o.Uses,
                    Creator = creator == null ? null : new(creator),
                    Guild = guild == null ? null : new(guild)
                };
            }).ToList();

            return Ok(invites);
        }
    }
}
