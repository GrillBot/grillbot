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

public class PointsController : ServiceControllerBase<IPointsServiceClient>
{
    public PointsController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpGet("{guildId}/leaderboard")]
    [ProducesResponseType(typeof(List<BoardItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetLeaderboardAsync([DiscordId, StringLength(32)] string guildId)
        => ExecuteAsync(async client => await client.GetLeaderboardAsync(guildId, 0, 0, EnumHelper.AggregateFlags<LeaderboardColumnFlag>(), LeaderboardSortOptions.ByTotalDescending));

    [HttpPost("list")]
    [ProducesResponseType(typeof(PaginatedResponse<TransactionItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetTransactionListAsync([FromBody] AdminListRequest request)
        => ExecuteAsync(async client => await client.GetTransactionListAsync(request), request);

    [HttpPost("list/chart")]
    [ProducesResponseType(typeof(List<PointsChartItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetChartDataAsync([FromBody] AdminListRequest request)
        => ExecuteAsync(async client => await client.GetChartDataAsync(request), request);

    [HttpGet("{guildId}/{userId}")]
    [ProducesResponseType(typeof(PointsStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetStatusOfPointsAsync(
        [DiscordId, StringLength(32)] string guildId,
        [DiscordId, StringLength(32)] string userId
    ) => ExecuteAsync(async client => await client.GetStatusOfPointsAsync(guildId, userId));

    [HttpPost("increment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> IncrementPointsAsync([FromBody] IncrementPointsRequest request)
        => ExecuteAsync(async client => await client.IncrementPointsAsync(request), request);

    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> TransferPointsAsync([FromBody] TransferPointsRequest request)
        => ExecuteAsync(async client => await client.TransferPointsAsync(request), request);

    [HttpDelete("{guildId}/{messageId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> DeleteTransactionAsync(
        [DiscordId, StringLength(32)] string guildId,
        [DiscordId, StringLength(32)] string messageId,
        [DiscordId, StringLength(32), FromQuery] string? reactionId = null
    ) => ExecuteRabbitPayloadAsync(() => new DeleteTransactionsPayload(guildId, messageId, reactionId));

    [HttpPost("list/users")]
    [ProducesResponseType(typeof(PaginatedResponse<UserListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> GetUserListAsync([FromBody] UserListRequest request)
        => ExecuteAsync(async client => await client.GetUserListAsync(request), request);
}
