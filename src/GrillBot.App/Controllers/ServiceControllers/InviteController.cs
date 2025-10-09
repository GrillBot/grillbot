using GrillBot.App.Actions.Api.V3.Services.Invite;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using InviteService;
using InviteService.Models.Request;
using InviteService.Models.Response;
using GrillBot.Core.Validation;
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
        => ExecuteAsync(async (client, ctx) => await client.GetCachedInvitesAsync(request, ctx.CancellationToken), request);

    [HttpPost("used-invites/list")]
    [ProducesResponseType<PaginatedResponse<Invite>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUsedInvitesAsync([FromBody] InviteListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetUsedInvitesAsync(request, ctx.CancellationToken), request);

    [HttpPost("invite-uses/list")]
    [ProducesResponseType<PaginatedResponse<InviteUse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetInviteUsesAsync([FromBody] InviteUseListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetInviteUsesAsync(request, ctx.CancellationToken), request);

    [HttpPost("user-invite-uses/list")]
    [ProducesResponseType<PaginatedResponse<UserInviteUse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserInviteUsesAsync([FromBody] UserInviteUseListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetUserInviteUsesAsync(request, ctx.CancellationToken), request);

    [HttpPost("synchronize/{guildId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> SynchronizeGuildInvitesAsync([FromRoute, DiscordId] ulong guildId)
        => ExecuteAsync<SynchronizeGuildInvitesAction>(guildId);
}
