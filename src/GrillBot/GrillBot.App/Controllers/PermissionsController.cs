using AutoMapper;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class PermissionsController : Controller
{
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PermissionsController(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
    {
        DiscordClient = discordClient;
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
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
        this.StoreParameters(parameters);
        await using var repository = DatabaseBuilder.CreateRepository();
        var exists = await repository.Permissions.ExistsCommandForTargetAsync(parameters.Command, parameters.TargetId);

        if (exists)
            return Conflict(new MessageResponse($"Explicitní oprávnění pro příkaz {parameters.Command} ({parameters.TargetId}) již existuje."));

        if (!char.IsLetter(parameters.Command[0]))
            parameters.Command = parameters.Command[1..];

        var permission = new Database.Entity.ExplicitPermission
        {
            IsRole = parameters.IsRole,
            TargetId = parameters.TargetId,
            Command = parameters.Command,
            State = parameters.State
        };

        await repository.AddAsync(permission);
        await repository.CommitAsync();
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
        await using var repository = DatabaseBuilder.CreateRepository();

        var permission = await repository.Permissions.FindPermissionForTargetAsync(command, targetId);
        if (permission == null)
            return NotFound(new MessageResponse($"Explicitní oprávnění pro příkaz {command} ({targetId}) neexistuje."));

        repository.Remove(permission);
        await repository.CommitAsync();
        return Ok();
    }

    /// <summary>
    /// Gets non-paginated list of explicit permissions.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("explicit")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<ExplicitPermission>>> GetExplicitPermissionsListAsync([FromQuery] string searchQuery)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var permissions = await repository.Permissions.GetPermissionsListAsync(searchQuery);

        var result = new List<ExplicitPermission>();

        var userPermissions = permissions.Where(o => !o.IsRole).ToList();
        if (userPermissions.Count > 0)
        {
            var users = await repository.User.FindAllUsersExceptBots();

            var userPerms = userPermissions
                .Select(o => Mapper.Map<ExplicitPermission>(o, x => x.AfterMap((_, dst) => dst.User = Mapper.Map<User>(users.Find(t => t.Id == o.TargetId)))))
                .OrderBy(o => o.User?.Username)
                .ThenBy(o => o.Command);

            result.AddRange(userPerms);
        }

        var rolePermissions = permissions.Where(o => o.IsRole).ToList();
        if (rolePermissions.Count > 0)
        {
            var roles = await DiscordClient.GetRolesAsync();
            var rolePerms = rolePermissions
                .Select(o => Mapper.Map<ExplicitPermission>(o, x => x.AfterMap((_, dst) => dst.Role = Mapper.Map<Role>(roles.Find(t => t.Id.ToString() == o.TargetId)))))
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
        await using var repository = DatabaseBuilder.CreateRepository();
        var permission = await repository.Permissions.FindPermissionForTargetAsync(command, targetId);
        if (permission == null)
            return NotFound(new MessageResponse($"Explicitní oprávnění pro příkaz {command} ({targetId}) neexistuje."));

        permission.State = state;
        await repository.CommitAsync();
        return Ok();
    }
}
