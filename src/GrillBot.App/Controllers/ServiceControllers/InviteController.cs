using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.InviteService;
using GrillBot.Core.Services.InviteService.Models.Request;
using GrillBot.Core.Services.InviteService.Models.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Invite(Admin)")]
public class InviteController(IServiceProvider serviceProvider) : ServiceControllerBase<IInviteServiceClient>(serviceProvider)
{
    [HttpPost("cached-invites/list")]
    [ProducesResponseType<PaginatedResponse<Invite>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetCachedInvitesAsync([FromBody] InviteListRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetCachedInvitesAsync(request, cancellationToken), request);

    [HttpPost("used-invites/list")]
    [ProducesResponseType<PaginatedResponse<Invite>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUsedInvitesAsync([FromBody] InviteListRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetUsedInvitesAsync(request, cancellationToken), request);

    [HttpPost("invite-uses/list")]
    [ProducesResponseType<PaginatedResponse<InviteUse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetInviteUsesAsync([FromBody] InviteUseListRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetInviteUsesAsync(request, cancellationToken), request);

    [HttpPost("user-invite-uses/list")]
    [ProducesResponseType<PaginatedResponse<UserInviteUse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserInviteUsesAsync([FromBody] UserInviteUseListRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetUserInviteUsesAsync(request, cancellationToken), request);
}
