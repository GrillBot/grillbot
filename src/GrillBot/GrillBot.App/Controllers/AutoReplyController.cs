using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/autoreply")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class AutoReplyController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public AutoReplyController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get nonpaginated list of auto replies.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AutoReplyItem>>> GetAutoReplyListAsync()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AutoReply.GetAutoReplyList>();
        var result = await action.ProcessAsync();

        return Ok(result);
    }

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
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AutoReply.GetAutoReplyItem>();
        var item = await action.ProcessAsync(id);

        return Ok(item);
    }

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

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AutoReply.CreateAutoReplyItem>();
        var result = await action.ProcessAsync(parameters);

        return Ok(result);
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

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AutoReply.UpdateAutoReplyItem>();
        var item = await action.ProcessAsync(id, parameters);

        return Ok(item);
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
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AutoReply.RemoveAutoReplyItem>();
        await action.ProcessAsync(id);

        return Ok();
    }
}
