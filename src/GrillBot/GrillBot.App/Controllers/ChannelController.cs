using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using GrillBot.App.Services.Channels;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Database.Models;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/channel")]
[OpenApiTag("Channels", Description = "Channel management")]
public class ChannelController : Controller
{
    private ChannelApiService ApiService { get; }

    public ChannelController(ChannelApiService apiService)
    {
        ApiService = apiService;
    }

    /// <summary>
    /// Send text message to channel.
    /// </summary>
    /// <param name="guildId">Guild Id.</param>
    /// <param name="channelId">Channel Id.</param>
    /// <param name="parameters"></param>
    /// <response code="200"></response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Guild or channel not exists.</response>
    [HttpPost("{guildId}/{channelId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SendMessageToChannelAsync(ulong guildId, ulong channelId, [FromBody] SendMessageToChannelParams parameters)
    {
        try
        {
            this.SetApiRequestData(parameters);
            await ApiService.PostMessageAsync(guildId, channelId, parameters);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new MessageResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get paginated list of channels.
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<GuildChannelListItem>>> GetChannelsListAsync([FromQuery] GetChannelListParams parameters)
    {
        var result = await ApiService.GetListAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Remove all messages in the message cache.
    /// </summary>
    [HttpDelete("{guildId}/{channelId}/cache")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ClearChannelCacheAsync(ulong guildId, ulong channelId)
    {
        await ApiService.ClearCacheAsync(guildId, channelId, User);
        return Ok();
    }

    /// <summary>
    /// Get detail of channel.
    /// </summary>
    /// <param name="id">Channel Id</param>
    /// <response code="200">Returns detail of channel.</response>
    /// <response code="404">Channel not found.</response>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChannelDetail>> GetChannelDetailAsync(ulong id)
    {
        var result = await ApiService.GetDetailAsync(id);

        if (result == null)
            return NotFound(new MessageResponse("Požadovaný kanál nebyl nalezen."));

        return Ok(result);
    }

    /// <summary>
    /// Update channel
    /// </summary>
    /// <response code="200"></response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Channel not found.</response>
    /// <response code="500">Something wrong.</response>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateChannelAsync(ulong id, [FromBody] UpdateChannelParams parameters)
    {
        try
        {
            this.SetApiRequestData(parameters);
            var result = await ApiService.UpdateChannelAsync(id, parameters);

            if (result)
                return Ok();

            return StatusCode(500, new MessageResponse("Nepodařilo se aktualizovat kanál."));
        }
        catch (NotFoundException)
        {
            return NotFound(new MessageResponse("Požadovaný kanál nebyl nalezen."));
        }
    }

    /// <summary>
    /// Get paginated list of user statistics in channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpGet("{id}/userStats")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<ChannelUserStatItem>>> GetChannelUsersAsync(ulong id, [FromQuery] PaginatedParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await ApiService.GetChannelUsersAsync(id, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Get channelboard for channels where user have access.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ResponseCache(CacheProfileName = "BoardApi")]
    public async Task<ActionResult<List<ChannelboardItem>>> GetChannelboardAsync()
    {
        var result = await ApiService.GetChannelBoardAsync();
        return Ok(result);
    }
}
