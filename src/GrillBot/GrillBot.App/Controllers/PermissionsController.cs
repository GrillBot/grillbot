using AutoMapper;
using GrillBot.App.Actions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Permissions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class PermissionsController : Controller
{
    private IDiscordClient DiscordClient { get; }
    private IMapper Mapper { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IServiceProvider ServiceProvider { get; }

    public PermissionsController(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder, IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        Mapper = mapper;
        DatabaseBuilder = databaseBuilder;
        ServiceProvider = serviceProvider;
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
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Command.CreateExplicitPermission>();
        await action.ProcessAsync(parameters);

        if (action.IsConflict)
            return Conflict(new MessageResponse(action.ErrorMessage));
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
