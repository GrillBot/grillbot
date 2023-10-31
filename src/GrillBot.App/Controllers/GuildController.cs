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
public class GuildController : Core.Infrastructure.Actions.ControllerBase
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
    [ProducesResponseType(typeof(PaginatedResponse<Guild>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetGuildListAsync([FromBody] GetGuildListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetGuildList>(parameters);
    }

    /// <summary>
    /// Get detailed information about guild.
    /// </summary>
    /// <param name="id">Guild ID</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GuildDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetGuildDetailAsync(ulong id)
        => await ProcessAsync<GetGuildDetail>(id);

    /// <summary>
    /// Update guild
    /// </summary>
    /// <response code="200">Return guild detail.</response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Guild not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(GuildDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> UpdateGuildAsync(ulong id, [FromBody] UpdateGuildParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateGuild>(id, parameters);
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
    [ProducesResponseType(typeof(ulong), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateScheduledEventAsync(ulong guildId, [FromBody] ScheduledEventParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateScheduledEvent>(guildId, parameters);
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
    public async Task<IActionResult> UpdateScheduledEventAsync(ulong guildId, ulong eventId, [FromBody] ScheduledEventParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateScheduledEvent>(guildId, eventId, parameters);
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
    public async Task<IActionResult> CancelScheduledEventAsync(ulong guildId, ulong eventId)
        => await ProcessAsync<CancelScheduledEvent>(guildId, eventId);
}
