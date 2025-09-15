using GrillBot.App.Actions.Api.V3.Unverify;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.GrillBot.Models;
using GrillBot.Core.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnverifyService;
using UnverifyService.Core.Enums;
using UnverifyService.Models.Request;
using UnverifyService.Models.Request.Keepables;
using UnverifyService.Models.Request.Logs;
using UnverifyService.Models.Request.Users;
using UnverifyService.Models.Response;
using UnverifyService.Models.Response.Guilds;
using UnverifyService.Models.Response.Keepables;
using UnverifyService.Models.Response.Logs;
using UnverifyService.Models.Response.Logs.Detail;
using UnverifyService.Models.Response.Users;

namespace GrillBot.App.Controllers.ServiceControllers;

[JwtAuthorize("Unverify(Admin)")]
public class UnverifyController(IServiceProvider serviceProvider) : ServiceControllerBase<IUnverifyServiceClient>(serviceProvider)
{
    [HttpGet("guild/{guildId}")]
    [ProducesResponseType<UnverifyService.Models.Response.Guilds.GuildInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetGuildInfoAsync([FromRoute, DiscordId] ulong guildId)
        => ExecuteAsync(async (client, ctx) => (await client.GetGuildInfoAsync(guildId, ctx.CancellationToken))!);

    [HttpPut("guild/{guildId}")]
    [ProducesResponseType<UnverifyService.Models.Response.Guilds.GuildInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> ModifyGuildAsync([FromRoute, DiscordId] ulong guildId, [FromBody] ModifyGuildRequest request)
        => ExecuteAsync(async (client, ctx) => (await client.ModifyGuildAsync(guildId, request, ctx.CancellationToken))!);

    [HttpPost("keepables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> CreateKeepablesAsync([FromBody] List<CreateKeepableRequest> requests)
        => ExecuteAsync(async (client, ctx) => await client.CreateKeepablesAsync(requests, ctx.CancellationToken));

    [HttpPost("keepables/list")]
    [ProducesResponseType<PaginatedResponse<KeepableListItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetKeepablesListAsync([FromBody] KeepablesListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetKeepablesListAsync(request, ctx.CancellationToken));

    [HttpDelete("keepables/{group}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> DeleteKeepablesAsync(
        [FromRoute, StringLength(100)] string group,
        [FromQuery, StringLength(100)] string? name
    ) => ExecuteAsync(async (client, ctx) => await client.DeleteKeepablesAsync(group, name, ctx.CancellationToken));

    [HttpPost("logs/list")]
    [ProducesResponseType<PaginatedResponse<UnverifyLogItem>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUnverifyLogsAsync([FromBody] UnverifyLogListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetUnverifyLogsAsync(request, ctx.CancellationToken));

    [HttpGet("logs/{id:guid}")]
    [ProducesResponseType<UnverifyLogDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetUnverifyLogDetailAsync(Guid id)
        => ExecuteAsync(async (client, ctx) => (await client.GetUnverifyLogDetailAsync(id, ctx.CancellationToken))!);

    [HttpGet("statistics/periodStats")]
    [ProducesResponseType<Dictionary<string, long>>(StatusCodes.Status200OK)]
    public Task<IActionResult> GetPeriodStatisticsAsync(
        [Required] string groupingKey,
        [Required] UnverifyOperationType operationType
    ) => ExecuteAsync(async (client, ctx) => await client.GetPeriodStatisticsAsync(groupingKey, operationType));

    [HttpPost("unverify/list")]
    [ProducesResponseType<PaginatedResponse<ActiveUnverifyListItemResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetActiveUnverifyListAsync([FromBody] ActiveUnverifyListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetActiveUnverifyListAsync(request, ctx.CancellationToken));

    [HttpGet("unverify/{guildId}/{userId}")]
    [ProducesResponseType<UnverifyDetail>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetActiveUnverifyDetailAsync([FromRoute, DiscordId] ulong guildId, [FromRoute, DiscordId] ulong userId)
        => ExecuteAsync(async (client, ctx) => (await client.GetActiveUnverifyDetailAsync(guildId, userId, ctx.CancellationToken))!);

    [HttpDelete("unverify/{guildId}/{userId}")]
    [ProducesResponseType<RemoveUnverifyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<RemoveUnverifyResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RemoveUnverifyAsync(
        [FromRoute, DiscordId] ulong guildId,
        [FromRoute, DiscordId] ulong userId,
        [FromQuery] bool isForceRemove
    ) => ExecuteAsync(async (client, ctx) => (await client.RemoveUnverifyAsync(guildId, userId, isForceRemove, ctx.AuthorizationToken, ctx.CancellationToken))!);

    [HttpPut("unverify")]
    [ProducesResponseType<LocalizedMessageContent>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<LocalizedMessageContent>(StatusCodes.Status404NotFound)]
    public Task<IActionResult> UpdateUnverifyAsync([FromBody] UpdateUnverifyRequest request)
        => ExecuteAsync(async (client, ctx) => (await client.UpdateUnverifyAsync(request, ctx.AuthorizationToken, ctx.CancellationToken))!);

    [HttpPut("unverify/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> ModifyUserAsync(
        [FromRoute, DiscordId] ulong userId,
        [FromBody] ModifyUserRequest request
    ) => ExecuteAsync(async (client, ctx) => await client.ModifyUserAsync(userId, request, ctx.AuthorizationToken, ctx.CancellationToken));

    [HttpGet("{userId}")]
    [ProducesResponseType<UserInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetUserInfoAsync(
        [FromRoute, DiscordId] ulong userId
    ) => ExecuteAsync(async (client, ctx) => (await client.GetUserInfoAsync(userId, ctx.CancellationToken))!);

    [HttpPost("logs/{id}/recover")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<LocalizedMessageContent>(StatusCodes.Status400BadRequest)]
    public Task<IActionResult> RecoverAccessAsync([FromRoute] Guid id)
        => ExecuteAsync<RecoverState>(id);
}
