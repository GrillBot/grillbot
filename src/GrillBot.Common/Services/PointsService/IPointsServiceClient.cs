using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Diagnostics.Models;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Common.Services.PointsService;

public interface IPointsServiceClient : IClient
{
    Task<RestResponse<PaginatedResponse<TransactionItem>>> GetTransactionListAsync(AdminListRequest request);
    Task<RestResponse<List<PointsChartItem>>> GetChartDataAsync(AdminListRequest request);
    Task<DiagnosticInfo> GetDiagAsync();
    Task<RestResponse<List<BoardItem>>> GetLeaderboardAsync(string guildId, int skip, int count);
    Task<MergeResult?> MergeTransctionsAsync();
    Task<PointsStatus> GetStatusOfPointsAsync(string guildId, string userId);
    Task<PointsStatus> GetStatusOfExpiredPointsAsync(string guildId, string userId);
    Task ProcessSynchronizationAsync(SynchronizationRequest request);
    Task<ValidationProblemDetails?> CreateTransactionAsync(TransactionRequest request);
    Task DeleteTransactionAsync(string guildId, string messageId);
    Task DeleteTransactionAsync(string guildId, string messageId, string reactionId);
    Task<ValidationProblemDetails?> TransferPointsAsync(TransferPointsRequest request);
    Task<ValidationProblemDetails?> CreateTransactionAsync(AdminTransactionRequest request);
}
