using GrillBot.App.Services.AutoReply;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AutoReply;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/autoreply")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[OpenApiTag("AutoReply", Description = "Auto response on discord messages")]
public class AutoReplyController : Controller
{
    private AutoReplyService AutoReplyService { get; }

    public AutoReplyController(AutoReplyService autoReplyService)
    {
        AutoReplyService = autoReplyService;
    }

    /// <summary>
    /// Gets nonpaginated list of auto replies.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(GetAutoReplyListAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AutoReplyItem>>> GetAutoReplyListAsync(CancellationToken cancellationToken)
    {
        var result = await AutoReplyService.GetListAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets reply item
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success</response>
    /// <response code="404">Reply not found</response>
    [HttpGet("{id}")]
    [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(GetItemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AutoReplyItem>> GetItemAsync(long id, CancellationToken cancellationToken)
    {
        var item = await AutoReplyService.GetItemAsync(id, cancellationToken);

        if (item == null)
            return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

        return Ok(item);
    }

    /// <summary>
    /// Creates new reply item.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost]
    [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(CreateItemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AutoReplyItem>> CreateItemAsync(AutoReplyItemParams parameters, CancellationToken cancellationToken)
    {
        var item = await AutoReplyService.CreateItemAsync(parameters, cancellationToken);
        return Ok(item);
    }

    /// <summary>
    /// Updates existing reply item.
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    /// <response code="404">Item not found</response>
    [HttpPut("{id}")]
    [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(UpdateItemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AutoReplyItem>> UpdateItemAsync(long id, [FromBody] AutoReplyItemParams parameters, CancellationToken cancellationToken)
    {
        var item = await AutoReplyService.UpdateItemAsync(id, parameters, cancellationToken);

        if (item == null)
            return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

        return Ok(item);
    }

    /// <summary>
    /// Removes reply item
    /// </summary>
    /// <param name="id">Reply ID</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success</response>
    /// <response code="404">Item not found</response>
    [HttpDelete("{id}")]
    [OpenApiOperation(nameof(AutoReplyController) + "_" + nameof(RemoveItemAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveItemAsync(long id, CancellationToken cancellationToken)
    {
        var result = await AutoReplyService.RemoveItemAsync(id, cancellationToken);

        if (!result)
            return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

        return Ok();
    }
}
