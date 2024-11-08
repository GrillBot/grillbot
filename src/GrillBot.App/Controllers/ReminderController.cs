using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Reminder;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Reminder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/remind")]
[ApiExplorerSettings(GroupName = "v1")]
public class ReminderController : Core.Infrastructure.Actions.ControllerBase
{
    public ReminderController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get paginated list of reminders.
    /// </summary>
    /// <response code="200">Returns paginated list of reminders.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "User")]
    [ProducesResponseType(typeof(PaginatedResponse<RemindMessage>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRemindMessagesListAsync([FromBody] GetReminderListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetReminderList>(parameters);
    }
}
