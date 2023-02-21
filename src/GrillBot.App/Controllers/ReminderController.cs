using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.Common.Models.Pagination;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.Reminder;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/remind")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class ReminderController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public ReminderController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get paginated list of reminders.
    /// </summary>
    /// <response code="200">Returns paginated list of reminders.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<RemindMessage>>> GetRemindMessagesListAsync([FromBody] GetReminderListParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Reminder.GetReminderList>();
        var result = await action.ProcessAsync(parameters);

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
    [HttpDelete("{id:long}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.Gone)]
    public async Task<ActionResult> CancelRemindAsync(long id, [FromQuery] bool notify = false)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Reminder.FinishRemind>();
        await action.ProcessAsync(id, notify, true);

        if (action.IsGone)
            return StatusCode((int)HttpStatusCode.Gone, new MessageResponse(action.ErrorMessage));
        return Ok();
    }
}
