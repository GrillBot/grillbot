using GrillBot.App.Services.Reminder;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/remind")]
[OpenApiTag("Reminder", Description = "Reminder management")]
public class ReminderController : Controller
{
    private RemindApiService ApiService { get; }

    public ReminderController(RemindApiService apiService)
    {
        ApiService = apiService;
    }

    /// <summary>
    /// Get paginated list of reminders.
    /// </summary>
    /// <response code="200">Returns paginated list of reminders.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpGet]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<RemindMessage>>> GetRemindMessagesListAsync([FromQuery] GetReminderListParams parameters)
    {
        if (User.HaveUserPermission())
        {
            parameters.ToUserId = User.GetUserId().ToString();
            parameters.OriginalMessageId = null;

            if (string.Equals(parameters.Sort.OrderBy, "ToUser", StringComparison.InvariantCultureIgnoreCase))
                parameters.Sort.OrderBy = "Id";
        }

        var result = await ApiService.GetListAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Cancel pending remind.
    /// </summary>
    /// <param name="id">Remind ID</param>
    /// <param name="notify">Send notification before cancel.</param>
    /// <response code="200">Success</response>
    /// <response code="404">Remind not found.</response>
    /// <response code="410">Remind was notified or cancelled.</response>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.Gone)]
    public async Task<ActionResult> CancelRemindAsync(long id, [FromQuery] bool notify = false)
    {
        try
        {
            await ApiService.ServiceCancellationAsync(id, notify);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new MessageResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode((int)HttpStatusCode.Gone, new MessageResponse(ex.Message));
        }
    }
}
