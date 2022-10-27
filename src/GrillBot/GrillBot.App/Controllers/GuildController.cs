using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Guilds.GuildEvents;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/guild")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class GuildController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public GuildController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get paginated list of guilds.
    /// </summary>
    /// <response code="200">Return paginated list of guilds in DB.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult<PaginatedResponse<Guild>>> GetGuildListAsync([FromBody] GetGuildListParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetGuildList>();
        var result = await action.ProcessAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get detailed information about guild.
    /// </summary>
    /// <param name="id">Guild ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult<GuildDetail>> GetGuildDetailAsync(ulong id)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.GetGuildDetail>();
        var result = await action.ProcessAsync(id);

        return Ok(result);
    }

    /// <summary>
    /// Update guild
    /// </summary>
    /// <response code="200">Return guild detail.</response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Guild not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult<GuildDetail>> UpdateGuildAsync(ulong id, [FromBody] UpdateGuildParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Guild.UpdateGuild>();
        var result = await action.ProcessAsync(id, parameters);

        return Ok(result);
    }

    /// <summary>
    /// Create a guild scheduled event.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="parameters">Event definition.</param>
    /// <response code="200">Success. Returns discord ID of the event.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Guild not found.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPost("{guildId}/event")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ulong>> CreateScheduledEventAsync(ulong guildId, [FromBody] ScheduledEventParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V2.Events.CreateScheduledEvent>();
        var result = await action.ProcessAsync(guildId, parameters);

        return Ok(result);
    }

    /// <summary>
    /// Update a guild scheduled event.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="eventId">Event ID</param>
    /// <param name="parameters">New definition of the event. Set only updated properties.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Guild or event wasn't found.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPatch("{guildId}/event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateScheduledEventAsync(ulong guildId, ulong eventId, [FromBody] ScheduledEventParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V2.Events.UpdateScheduledEvent>();
        await action.ProcessAsync(guildId, eventId, parameters);

        return Ok();
    }
}
