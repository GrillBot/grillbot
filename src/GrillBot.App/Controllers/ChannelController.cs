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
public class ChannelController : Infrastructure.ControllerBase
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<ChannelUserStatItem>>> GetChannelUsersAsync(ulong id, [FromBody] PaginatedParams pagination)
    {
        ApiAction.Init(this, pagination);

        return Ok(await ProcessActionAsync<GetChannelUsers, PaginatedResponse<ChannelUserStatItem>>(action => action.ProcessAsync(id, pagination)));
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

        await ProcessActionAsync<SendMessageToChannel>(action => action.ProcessAsync(guildId, channelId, parameters));
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

        return Ok(await ProcessActionAsync<GetChannelList, PaginatedResponse<GuildChannelListItem>>(action => action.ProcessAsync(parameters)));
    }

    /// <summary>
    /// Remove all messages in the message cache.
    /// </summary>
    [HttpDelete("{guildId}/{channelId}/cache")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ClearChannelCacheAsync(ulong guildId, ulong channelId)
    {
        await ProcessActionAsync<ClearMessageCache>(action => action.ProcessAsync(guildId, channelId));
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
        => Ok(await ProcessActionAsync<GetChannelDetail, ChannelDetail>(action => action.ProcessAsync(id)));

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

        await ProcessActionAsync<UpdateChannel>(action => action.ProcessAsync(id, parameters));
        return Ok();
    }

    /// <summary>
    /// Get channelboard for channels where user have access.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("board")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ChannelboardItem>>> GetChannelboardAsync()
        => Ok(await ProcessActionAsync<GetChannelboard, List<ChannelboardItem>>(action => action.ProcessAsync()));

    /// <summary>
    /// Get all pins from channel.
    /// </summary>
    /// <response code="200">Returns pins in the channel in markdown or json format.</response>
    /// <response code="404">Guild wasn't found.</response>
    [HttpGet("{channelId}/pins")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetChannelPinsAsync(ulong channelId, bool markdown)
    {
        return Content(
            await ProcessActionAsync<GetPins, string>(action => action.ProcessAsync(channelId, markdown)),
            markdown ? "text/markdown" : "application/json"
        );
    }

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
        => File(await ProcessActionAsync<GetPinsWithAttachments, byte[]>(action => action.ProcessAsync(channelId)), "application/zip");
}
