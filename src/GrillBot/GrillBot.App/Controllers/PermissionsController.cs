using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[OpenApiTag("Permissions", Description = "Commands permissions management")]
public class PermissionsController : Controller
{
    private GrillBotContext DbContext { get; }
    private DiscordSocketClient DiscordClient { get; }
    private IMapper Mapper { get; }

    public PermissionsController(GrillBotContext dbContext, DiscordSocketClient discordClient, IMapper mapper)
    {
        DbContext = dbContext;
        DiscordClient = discordClient;
        Mapper = mapper;
    }

    /// <summary>
    /// Creates explicit permission.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">Permission exists.</response>
    [HttpPost("explicit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.Conflict)]
    public async Task<ActionResult> CreateExplicitPermissionAsync([FromBody] CreateExplicitPermissionParams parameters)
    {
        this.SetApiRequestData(parameters);
        var exists = await DbContext.ExplicitPermissions.AsNoTracking()
            .AnyAsync(o => o.Command == parameters.Command && o.TargetId == parameters.TargetId);

        if (exists)
            return Conflict(new MessageResponse($"Explicitní oprávnění pro příkaz {parameters.Command} ({parameters.TargetId}) již existuje."));

        if (!char.IsLetter(parameters.Command[0]))
            parameters.Command = parameters.Command[1..];

        var permission = new Database.Entity.ExplicitPermission()
        {
            IsRole = parameters.IsRole,
            TargetId = parameters.TargetId,
            Command = parameters.Command,
            State = parameters.State
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
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<ExplicitPermission>>> GetExplicitPermissionsListAsync([FromQuery] string searchQuery,
        CancellationToken cancellationToken)
    {
        var query = DbContext.ExplicitPermissions.AsNoTracking();

        if (!string.IsNullOrEmpty(searchQuery))
            query = query.Where(o => o.Command.Contains(searchQuery));

        var items = await query.ToListAsync(cancellationToken);
        var result = new List<ExplicitPermission>();

        var userPermissions = items.Where(o => !o.IsRole);
        if (userPermissions.Any())
        {
            var users = await DbContext.Users.AsNoTracking()
                .Where(o => (o.Flags & (int)UserFlags.NotUser) == 0)
                .Select(o => new Database.Entity.User() { Id = o.Id, Username = o.Username, Discriminator = o.Discriminator })
                .ToListAsync(cancellationToken);

            var userPerms = userPermissions
                .Select(o => Mapper.Map<ExplicitPermission>(o, x => x.AfterMap((_, dst) => dst.User = Mapper.Map<User>(users.Find(x => x.Id == o.TargetId)))))
                .OrderBy(o => o.User?.Username)
                .ThenBy(o => o.Command);

            result.AddRange(userPerms);
        }

        var rolePermissions = items.Where(o => o.IsRole);
        if (rolePermissions.Any())
        {
            var rolePerms = rolePermissions
                .Select(o => Mapper.Map<ExplicitPermission>(o, x => x.AfterMap((_, dst) => dst.Role = Mapper.Map<Role>(DiscordClient.FindRole(o.TargetId)))))
                .OrderBy(o => o.Role?.Name)
                .ThenBy(o => o.Command);

            result.AddRange(rolePerms);
        }

        return Ok(result);
    }

    /// <summary>
    /// Sets permission state of command.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">Permission not found.</response>
    [HttpPut("set")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult> SetExplicitPermissionStateAsync([Required] string command, [Required] string targetId, ExplicitPermissionState state)
    {
        var permission = await DbContext.ExplicitPermissions.AsQueryable()
            .FirstOrDefaultAsync(o => o.Command == command && o.TargetId == targetId);

        if (permission == null)
            return NotFound(new MessageResponse($"Explicitní oprávnění pro příkaz {command} ({targetId}) neexistuje."));

        permission.State = state;
        await DbContext.SaveChangesAsync();
        return Ok();
    }
}
