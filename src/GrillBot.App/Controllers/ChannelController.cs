using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Channel;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/channel")]
[ApiExplorerSettings(GroupName = "v1")]
public class ChannelController : Core.Infrastructure.Actions.ControllerBase
{
    public ChannelController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get paginated list of user statistics in channel.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("{id}/userStats")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<ChannelUserStatItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<IActionResult> GetChannelUsersAsync(ulong id, [FromBody] PaginatedParams pagination)
    {
        ApiAction.Init(this, pagination);
        return await ProcessAsync<GetChannelUsers>(id, pagination);
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
    public async Task<IActionResult> SendMessageToChannelAsync(ulong guildId, ulong channelId, [FromBody] SendMessageToChannelParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<SendMessageToChannel>(guildId, channelId, parameters);
    }

    /// <summary>
    /// Get paginated list of channels.
    /// </summary>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(PaginatedResponse<GuildChannelListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetChannelsListAsync([FromBody] GetChannelListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetChannelList>(parameters);
    }

    /// <summary>
    /// Remove all messages in the message cache.
    /// </summary>
    [HttpDelete("{guildId}/{channelId}/cache")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearChannelCacheAsync(ulong guildId, ulong channelId)
        => await ProcessAsync<ClearMessageCache>(guildId, channelId);

    /// <summary>
    /// Get detail of channel.
    /// </summary>
    /// <param name="id">Channel Id</param>
    /// <response code="200">Returns detail of channel.</response>
    /// <response code="404">Channel not found.</response>
    [HttpGet("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(ChannelDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChannelDetailAsync(ulong id)
        => await ProcessAsync<GetChannelDetail>(id);

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
    public async Task<IActionResult> UpdateChannelAsync(ulong id, [FromBody] UpdateChannelParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateChannel>(id, parameters);
    }

    /// <summary>
    /// Get channelboard for channels where user have access.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(List<ChannelboardItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChannelboardAsync()
        => await ProcessAsync<GetChannelboard>();

    /// <summary>
    /// Get all pins from channel.
    /// </summary>
    /// <response code="200">Returns pins in the channel in markdown or json format.</response>
    /// <response code="404">Guild wasn't found.</response>
    [HttpGet("{channelId}/pins")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChannelPinsAsync(ulong channelId, bool markdown)
        => await ProcessAsync<GetPins>(channelId, markdown);

    /// <summary>
    /// Get all pins with attachments as zip archive.
    /// </summary>
    /// <response code="200">Returns pins and attachments in the channel in the zip.</response>
    /// <response code="404">Guild wasn't found.</response>
    [HttpGet("{channelId}/pins/attachments")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChannelPinsWithAttachmentsAsync(ulong channelId)
        => await ProcessAsync<GetPinsWithAttachments>(channelId);
}
