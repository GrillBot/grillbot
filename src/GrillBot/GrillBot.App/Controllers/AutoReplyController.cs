using GrillBot.App.Services.AutoReply;
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
public class AutoReplyController : Controller
{
    private AutoReplyApiService AutoReplyApiService { get; }

    public AutoReplyController(AutoReplyApiService autoReplyApiService)
    {
        AutoReplyApiService = autoReplyApiService;
    }

    /// <summary>
    /// Get nonpaginated list of auto replies.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AutoReplyItem>>> GetAutoReplyListAsync()
    {
        var result = await AutoReplyApiService.GetListAsync();
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
        var item = await AutoReplyApiService.GetItemAsync(id);

        if (item == null)
            return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

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
        this.StoreParameters(parameters);

        var item = await AutoReplyApiService.CreateItemAsync(parameters);
        return Ok(item);
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
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AutoReplyItem>> UpdateItemAsync(long id, [FromBody] AutoReplyItemParams parameters)
    {
        this.StoreParameters(parameters);
        var item = await AutoReplyApiService.UpdateItemAsync(id, parameters);

        if (item == null)
            return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

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
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveItemAsync(long id)
    {
        var result = await AutoReplyApiService.RemoveItemAsync(id);

        if (!result)
            return NotFound(new MessageResponse($"Požadovaná automatická odpověď s ID {id} nebyla nalezena."));

        return Ok();
    }
}
