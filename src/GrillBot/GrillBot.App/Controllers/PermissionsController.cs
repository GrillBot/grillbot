using Discord.WebSocket;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [OpenApiTag("Permissions", Description = "Commands permissions management")]
    public class PermissionsController : Controller
    {
        private GrillBotContext DbContext { get; }
        private DiscordSocketClient DiscordClient { get; }

        public PermissionsController(GrillBotContext dbContext, DiscordSocketClient discordClient)
        {
            DbContext = dbContext;
            DiscordClient = discordClient;
        }

        /// <summary>
        /// Creates explicit permission.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="409">Permission exists.</response>
        [HttpPost("explicit")]
        [OpenApiOperation(nameof(PermissionsController) + "_" + nameof(CreateExplicitPermissionAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateExplicitPermissionAsync([FromBody] CreateExplicitPermissionParams parameters)
        {
            var exists = await DbContext.ExplicitPermissions.AsNoTracking()
                .AnyAsync(o => o.Command == parameters.Command && o.TargetId == parameters.TargetId);

            if (exists)
                return Conflict(new MessageResponse($"Explicitní oprávnění pro příkaz {parameters.Command} ({parameters.TargetId}) již existuje."));

            var permission = new Database.Entity.ExplicitPermission()
            {
                IsRole = parameters.IsRole,
                TargetId = parameters.TargetId,
                Command = parameters.Command
            };

            await DbContext.AddAsync(permission);
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Removes explicit permission
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Permission not found</response>
        [HttpDelete("explicit")]
        [OpenApiOperation(nameof(PermissionsController) + "_" + nameof(RemoveExplicitPermissionAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> RemoveExplicitPermissionAsync([Required] string command, [Required] string targetId)
        {
            var permission = await DbContext.ExplicitPermissions.AsQueryable()
                .FirstOrDefaultAsync(o => o.Command == command && o.TargetId == targetId);

            if (permission == null)
                return NotFound(new MessageResponse($"Explicitní oprávnění pro příkaz {command} ({targetId}) neexistuje."));

            DbContext.Remove(permission);
            await DbContext.SaveChangesAsync();
            return Ok();
        }

        /// <summary>
        /// Gets non-paginated list of explicit permissions.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("explicit")]
        [OpenApiOperation(nameof(PermissionsController) + "_" + nameof(GetExplicitPermissionsListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<Data.Models.API.Permissions.ExplicitPermission>>> GetExplicitPermissionsListAsync([FromQuery] string searchQuery)
        {
            var query = DbContext.ExplicitPermissions.AsNoTracking();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(o => o.Command.Contains(searchQuery));

            var items = await query.ToListAsync();
            var result = new List<Data.Models.API.Permissions.ExplicitPermission>();

            var userPermissions = items.Where(o => !o.IsRole);
            if (userPermissions.Any())
            {
                var users = await DbContext.Users.AsNoTracking()
                    .Where(o => (o.Flags & (int)UserFlags.NotUser) == 0)
                    .Select(o => new User() { Flags = o.Flags, Id = o.Id, Username = o.Username })
                    .ToListAsync();

                result.AddRange(userPermissions.Select(o =>
                {
                    var user = users.Find(x => x.Id == o.TargetId);
                    return new Data.Models.API.Permissions.ExplicitPermission(o, user, null);
                }));
            }

            var rolePermissions = items.Where(o => o.IsRole);
            if (rolePermissions.Any())
            {
                result.AddRange(rolePermissions.Select(o =>
                    new Data.Models.API.Permissions.ExplicitPermission(o, null, new Role(DiscordClient.FindRole(Convert.ToUInt64(o.TargetId))))));
            }

            result = result.OrderBy(o => o.Command).ToList();
            return Ok(result);
        }
    }
}
