using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers
{
    [ApiController]
    [Route("api/permissions")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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
        public async Task<ActionResult> CreateExplicitPermissionAsync([FromBody] CreateExplicitPermissionParams parameters, CancellationToken cancellationToken)
        {
            var exists = await DbContext.ExplicitPermissions.AsNoTracking()
                .AnyAsync(o => o.Command == parameters.Command && o.TargetId == parameters.TargetId, cancellationToken);

            if (exists)
                return Conflict(new MessageResponse($"Explicitní oprávnění pro příkaz {parameters.Command} ({parameters.TargetId}) již existuje."));

            var permission = new Database.Entity.ExplicitPermission()
            {
                IsRole = parameters.IsRole,
                TargetId = parameters.TargetId,
                Command = parameters.Command,
                State = parameters.State
            };

            await DbContext.AddAsync(permission, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);
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
        public async Task<ActionResult> RemoveExplicitPermissionAsync([Required] string command, [Required] string targetId, CancellationToken cancellationToken)
        {
            var permission = await DbContext.ExplicitPermissions.AsQueryable()
                .FirstOrDefaultAsync(o => o.Command == command && o.TargetId == targetId, cancellationToken);

            if (permission == null)
                return NotFound(new MessageResponse($"Explicitní oprávnění pro příkaz {command} ({targetId}) neexistuje."));

            DbContext.Remove(permission);
            await DbContext.SaveChangesAsync(cancellationToken);
            return Ok();
        }

        /// <summary>
        /// Gets non-paginated list of explicit permissions.
        /// </summary>
        /// <response code="200">Success</response>
        [HttpGet("explicit")]
        [OpenApiOperation(nameof(PermissionsController) + "_" + nameof(GetExplicitPermissionsListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<Data.Models.API.Permissions.ExplicitPermission>>> GetExplicitPermissionsListAsync([FromQuery] string searchQuery,
            CancellationToken cancellationToken)
        {
            var query = DbContext.ExplicitPermissions.AsNoTracking();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(o => o.Command.Contains(searchQuery));

            var items = await query.ToListAsync(cancellationToken);
            var result = new List<Data.Models.API.Permissions.ExplicitPermission>();

            var userPermissions = items.Where(o => !o.IsRole);
            if (userPermissions.Any())
            {
                var users = await DbContext.Users.AsNoTracking()
                    .Where(o => (o.Flags & (int)UserFlags.NotUser) == 0)
                    .Select(o => new User() { Flags = o.Flags, Id = o.Id, Username = o.Username, Discriminator = o.Discriminator })
                    .ToListAsync(cancellationToken);

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
                {
                    var role = DiscordClient.FindRole(Convert.ToUInt64(o.TargetId));
                    return new Data.Models.API.Permissions.ExplicitPermission(o, null, role != null ? new Role(role) : null);
                }));
            }

            result = result.OrderBy(o => o.Command).ToList();
            return Ok(result);
        }

        /// <summary>
        /// Sets permission state of command.
        /// </summary>
        /// <response code="200">Success</response>
        /// <response code="404">Permission not found.</response>
        [HttpPut("set")]
        [OpenApiOperation(nameof(PermissionsController) + "_" + nameof(GetExplicitPermissionsListAsync))]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> SetExplicitPermissionStateAsync([Required] string command, [Required] string targetId, ExplicitPermissionState state,
            CancellationToken cancellationToken)
        {
            var permission = await DbContext.ExplicitPermissions.AsQueryable()
                .FirstOrDefaultAsync(o => o.Command == command && o.TargetId == targetId, cancellationToken);

            if (permission == null)
                return NotFound(new MessageResponse($"Explicitní oprávnění pro příkaz {command} ({targetId}) neexistuje."));

            permission.State = state;
            await DbContext.SaveChangesAsync(cancellationToken);
            return Ok();
        }
    }
}
