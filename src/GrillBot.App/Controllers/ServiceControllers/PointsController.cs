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
        => ExecuteAsync(async (client, cancellationToken) => await client.GetLeaderboardAsync(guildId, 0, 0, EnumHelper.AggregateFlags<LeaderboardColumnFlag>(), LeaderboardSortOptions.ByTotalDescending, cancellationToken));

    [HttpPost("list")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetTransactionListAsync([FromBody] AdminListRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetTransactionListAsync(request, cancellationToken), request);

    [HttpPost("list/chart")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(typeof(List<PointsChartItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetChartDataAsync([FromBody] AdminListRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.GetChartDataAsync(request, cancellationToken), request);

    [HttpGet("{guildId}/{userId}")]
    [JwtAuthorize("Points(Admin)", "Points(UserStatus)")]
    [ProducesResponseType(typeof(PointsStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetStatusOfPointsAsync(
        [DiscordId, StringLength(32)] string guildId,
        [DiscordId, StringLength(32)] string userId
    ) => ExecuteAsync(async (client, cancellationToken) => await client.GetStatusOfPointsAsync(guildId, userId, cancellationToken));

    [HttpPost("increment")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> IncrementPointsAsync([FromBody] IncrementPointsRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.IncrementPointsAsync(request, cancellationToken), request);

    [HttpPost("transfer")]
    [JwtAuthorize("Points(Admin)")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> TransferPointsAsync([FromBody] TransferPointsRequest request)
        => ExecuteAsync(async (client, cancellationToken) => await client.TransferPointsAsync(request, cancellationToken), request);

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
        => ExecuteAsync(async (client, cancellationToken) => await client.GetUserListAsync(request, cancellationToken), request);
}
