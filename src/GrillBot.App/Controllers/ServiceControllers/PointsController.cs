using GrillBot.App.Infrastructure.Auth;
using GrillBot.Common.Helpers;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Enums;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Services.PointsService.Models.Events;
using GrillBot.Core.Services.PointsService.Models.Users;
using GrillBot.Core.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers.ServiceControllers;

public class PointsController(IServiceProvider serviceProvider) : ServiceControllerBase<IPointsServiceClient>(serviceProvider)
{
    [HttpGet("{guildId}/leaderboard")]
    [JwtAuthorize("Points(Admin)", "Points(Leaderboard)")]
    [ProducesResponseType(typeof(List<BoardItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetLeaderboardAsync([DiscordId, StringLength(32)] string guildId)
        => ExecuteAsync(async (client, ctx) => await client.GetLeaderboardAsync(guildId, 0, 0, EnumHelper.AggregateFlags<LeaderboardColumnFlag>(), LeaderboardSortOptions.ByTotalDescending, ctx.CancellationToken));

    [HttpPost("list")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetTransactionListAsync([FromBody] AdminListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetTransactionListAsync(request, ctx.CancellationToken), request);

    [HttpPost("list/chart")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(typeof(List<PointsChartItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetChartDataAsync([FromBody] AdminListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetChartDataAsync(request, ctx.CancellationToken), request);

    [HttpGet("{guildId}/{userId}")]
    [JwtAuthorize("Points(Admin)", "Points(UserStatus)")]
    [ProducesResponseType(typeof(PointsStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetStatusOfPointsAsync(
        [DiscordId, StringLength(32)] string guildId,
        [DiscordId, StringLength(32)] string userId
    ) => ExecuteAsync(async (client, ctx) => await client.GetStatusOfPointsAsync(guildId, userId, ctx.CancellationToken));

    [HttpPost("increment")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> IncrementPointsAsync([FromBody] IncrementPointsRequest request)
        => ExecuteAsync(async (client, ctx) => await client.IncrementPointsAsync(request, ctx.CancellationToken), request);

    [HttpPost("transfer")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> TransferPointsAsync([FromBody] TransferPointsRequest request)
        => ExecuteAsync(async (client, ctx) => await client.TransferPointsAsync(request, ctx.CancellationToken), request);

    [HttpDelete("{guildId}/{messageId}")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> DeleteTransactionAsync(
        [DiscordId, StringLength(32)] string guildId,
        [DiscordId, StringLength(32)] string messageId,
        [FromQuery] string? reactionId = null
    ) => ExecuteRabbitPayloadAsync(() => new DeleteTransactionsPayload(guildId, messageId, reactionId));

    [HttpPost("list/users")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(typeof(PaginatedResponse<UserListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserListAsync([FromBody] UserListRequest request)
        => ExecuteAsync(async (client, ctx) => await client.GetUserListAsync(request, ctx.CancellationToken), request);
}
