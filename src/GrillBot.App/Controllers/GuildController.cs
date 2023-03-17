using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.App.Actions.Api.V2.Events;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Guilds.GuildEvents;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/guild")]
[ApiExplorerSettings(GroupName = "v1")]
public class GuildController : Infrastructure.ControllerBase
{
    public GuildController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
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

        return Ok(await ProcessActionAsync<GetGuildList, PaginatedResponse<Guild>>(action => action.ProcessAsync(parameters)));
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
        => Ok(await ProcessActionAsync<GetGuildDetail, GuildDetail>(action => action.ProcessAsync(id)));

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
        return Ok(await ProcessActionAsync<UpdateGuild, GuildDetail>(action => action.ProcessAsync(id, parameters)));
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
        return Ok(await ProcessActionAsync<CreateScheduledEvent, ulong>(action => action.ProcessAsync(guildId, parameters)));
    }

    /// <summary>
    /// Update a guild scheduled event.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="eventId">Event ID</param>
    /// <param name="parameters">New definition of the event. Set only updated properties.</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="403">Event wasn't created by a bot</response>
    /// <response code="404">Guild or event wasn't found.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPatch("{guildId}/event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateScheduledEventAsync(ulong guildId, ulong eventId, [FromBody] ScheduledEventParams parameters)
    {
        ApiAction.Init(this, parameters);

        await ProcessActionAsync<UpdateScheduledEvent>(action => action.ProcessAsync(guildId, eventId, parameters));
        return Ok();
    }

    /// <summary>
    /// Cancel a guild scheduled event.
    /// </summary>
    /// <param name="guildId">Guild ID</param>
    /// <param name="eventId">Event ID</param>
    /// <response code="200">Success.</response>
    /// <response code="400">Validation failed. Event finished or is cancelled.</response>
    /// <response code="403">Event wasn't created by a bot.</response>
    /// <response code="404">Guild or event wasn't found.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpDelete("{guildId}/event/{eventId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelScheduledEventAsync(ulong guildId, ulong eventId)
    {
        await ProcessActionAsync<CancelScheduledEvent>(action => action.ProcessAsync(guildId, eventId));
        return Ok();
    }
}
