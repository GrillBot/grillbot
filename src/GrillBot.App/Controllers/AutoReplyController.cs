using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.AutoReply;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/autoreply")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class AutoReplyController : Core.Infrastructure.Actions.ControllerBase
{
    public AutoReplyController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get nonpaginated list of auto replies.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<AutoReplyItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAutoReplyListAsync()
        => await ProcessAsync<GetAutoReplyList>();

    /// <summary>
    /// Get reply item
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <response code="200">Success</response>
    /// <response code="404">Reply not found</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AutoReplyItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemAsync(long id)
        => await ProcessAsync<GetAutoReplyList>(id);

    /// <summary>
    /// Create new reply item.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [ProducesResponseType(typeof(AutoReplyItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateItemAsync([FromBody] AutoReplyItemParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateAutoReplyItem>(parameters);
    }

    /// <summary>
    /// Update existing reply item.
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <param name="parameters"></param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    /// <response code="404">Item not found</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(AutoReplyItem), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemAsync(long id, [FromBody] AutoReplyItemParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateAutoReplyItem>(id, parameters);
    }

    /// <summary>
    /// Remove reply item
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <response code="200">Success</response>
    /// <response code="404">Item not found</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItemAsync(long id)
        => await ProcessAsync<RemoveAutoReplyItem>(id);
}
