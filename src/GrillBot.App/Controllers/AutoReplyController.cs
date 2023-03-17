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
public class AutoReplyController : Infrastructure.ControllerBase
{
    public AutoReplyController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get nonpaginated list of auto replies.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AutoReplyItem>>> GetAutoReplyListAsync()
        => Ok(await ProcessActionAsync<GetAutoReplyList, List<AutoReplyItem>>(action => action.ProcessAsync()));

    /// <summary>
    /// Get reply item
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <response code="200">Success</response>
    /// <response code="404">Reply not found</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutoReplyItem>> GetItemAsync(long id)
        => Ok(await ProcessActionAsync<GetAutoReplyItem, AutoReplyItem>(action => action.ProcessAsync(id)));

    /// <summary>
    /// Create new reply item.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutoReplyItem>> CreateItemAsync([FromBody] AutoReplyItemParams parameters)
    {
        ApiAction.Init(this, parameters);

        return Ok(await ProcessActionAsync<CreateAutoReplyItem, AutoReplyItem>(action => action.ProcessAsync(parameters)));
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutoReplyItem>> UpdateItemAsync(long id, [FromBody] AutoReplyItemParams parameters)
    {
        ApiAction.Init(this, parameters);

        return Ok(await ProcessActionAsync<UpdateAutoReplyItem, AutoReplyItem>(action => action.ProcessAsync(id, parameters)));
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
    public async Task<ActionResult> RemoveItemAsync(long id)
    {
        await ProcessActionAsync<RemoveAutoReplyItem>(action => action.ProcessAsync(id));
        return Ok();
    }
}
