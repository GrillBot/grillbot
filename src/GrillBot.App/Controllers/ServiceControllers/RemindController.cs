using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.RemindService;
using GrillBot.Core.Services.RemindService.Models.Request;
using GrillBot.Core.Services.RemindService.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

public class RemindController(IServiceProvider serviceProvider) : ServiceControllerBase<IRemindServiceClient>(serviceProvider)
{
    [HttpPost("list")]
    [JwtAuthorize("Remind(Admin)", "Remind(OnlyMyReminders)")]
    [ProducesResponseType(typeof(PaginatedResponse<RemindMessageItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetReminderListAsync(ReminderListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetReminderListAsync(request, ctx.CancellationToken), request);

    [HttpPut("cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [JwtAuthorize("Remind(Admin)", "Remind(CancelMyReminders)")]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CancelRemindAsync(CancelReminderRequest request)
        => ExecuteAsync(async (client, ctx) => await client.CancelReminderAsync(request, ctx.CancellationToken), request);
}
