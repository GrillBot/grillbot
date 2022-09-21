using GrillBot.App.Actions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GrillBot.App.Services.Channels;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/channel")]
[ApiExplorerSettings(GroupName = "v1")]
public class ChannelController : Controller
{
    private ChannelApiService ApiService { get; }
    private IServiceProvider ServiceProvider { get; }

    public ChannelController(ChannelApiService apiService, IServiceProvider serviceProvider)
    {
        ApiService = apiService;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get paginated list of user statistics in channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("{id}/userStats")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<ChannelUserStatItem>>> GetChannelUsersAsync(ulong id, [FromBody] PaginatedParams pagination)
    {
        ApiAction.Init(this, pagination);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.GetChannelUsers>();
        var result = await action.ProcessAsync(id, pagination);
        return Ok(result);
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
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.SendMessageToChannel>();
        await action.ProcessAsync(guildId, channelId, parameters);
        return Ok();
    }

    /// <summary>
    /// Get paginated list of channels.
    /// </summary>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<GuildChannelListItem>>> GetChannelsListAsync([FromBody] GetChannelListParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.GetChannelList>();
        var result = await action.ProcessAsync(parameters);
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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.ClearMessageCache>();
        await action.ProcessAsync(guildId, channelId);
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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.GetChannelDetail>();
        var result = await action.ProcessAsync(id);
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
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Channel.UpdateChannel>();
        await action.ProcessAsync(id, parameters);
        return Ok();
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
