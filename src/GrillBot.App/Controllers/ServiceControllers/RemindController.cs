using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;
using GrillBot.Core.Services.RemindService.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

public class RemindController : ServiceControllerBase<IRemindServiceClient>
{
    public RemindController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpPost("list")]
    [JwtAuthorize("Remind(Admin)", "Remind(OnlyMyReminders)")]
    [ProducesResponseType(typeof(PaginatedResponse<RemindMessageItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetReminderListAsync(ReminderListRequest request)
        => ExecuteAsync(async client => await client.GetReminderListAsync(request), request);

    [HttpPut("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [JwtAuthorize("Remind(Admin)", "Remind(CancelMyReminders)")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CancelRemindAsync(CancelReminderRequest request)
        => ExecuteAsync(async client => await client.CancelReminderAsync(request), request);
}
