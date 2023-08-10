using System.Net;
using System.Net.Http.Json;
using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.PointsService.Models;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Diagnostics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GrillBot.Common.Services.PointsService;

public class PointsServiceClient : RestServiceBase, IPointsServiceClient
{
    public override string ServiceName => "PointsService";

    public PointsServiceClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, httpClientFactory)
    {
    }

    public async Task<RestResponse<PaginatedResponse<TransactionItem>>> GetTransactionListAsync(AdminListRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/admin/list", request, cancellationToken),
            ReadRestResponseAsync<PaginatedResponse<TransactionItem>>,
            async (response, cancellationToken) =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response, cancellationToken);
            }
        );
    }

    public async Task<RestResponse<List<PointsChartItem>>> GetChartDataAsync(AdminListRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/chart", request, cancellationToken),
            ReadRestResponseAsync<List<PointsChartItem>>,
            async (response, cancellationToken) =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response, cancellationToken);
            }
        );
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag", cancellationToken), ReadJsonAsync<DiagnosticInfo>);

    public async Task<RestResponse<List<BoardItem>>> GetLeaderboardAsync(string guildId, int skip, int count, bool simple)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync($"api/leaderboard/{guildId}?skip={skip}&count={count}&simple={(simple ? "true" : "false")}", cancellationToken),
            ReadRestResponseAsync<List<BoardItem>>,
            async (response, cancellationToken) =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response, cancellationToken);
            }
        );
    }

    public async Task<int> GetLeaderboardCountAsync(string guildId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/leaderboard/{guildId}/count", cancellationToken), ReadJsonAsync<int>);

    public async Task<MergeResult?> MergeTransctionsAsync()
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsync("api/merge", null, cancellationToken),
            ReadJsonAsync<MergeResult>,
            timeout: System.Threading.Timeout.InfiniteTimeSpan
        );
    }

    public async Task<PointsStatus> GetStatusOfPointsAsync(string guildId, string userId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/status/{guildId}/{userId}", cancellationToken), ReadJsonAsync<PointsStatus>);

    public async Task<PointsStatus> GetStatusOfExpiredPointsAsync(string guildId, string userId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/status/{guildId}/{userId}/expired", cancellationToken), ReadJsonAsync<PointsStatus>);

    public async Task<ImagePointsStatus?> GetImagePointsStatusAsync(string guildId, string userId)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync($"api/status/{guildId}/{userId}/image", cancellationToken),
            async (response, cancellationToken) => response.StatusCode == HttpStatusCode.NotFound ? null : await ReadJsonAsync<ImagePointsStatus>(response, cancellationToken),
            (response, cancellationToken) => response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound ? Task.CompletedTask : EnsureSuccessResponseAsync(response, cancellationToken)
        );
    }

    public async Task ProcessSynchronizationAsync(SynchronizationRequest request)
        => await ProcessRequestAsync(cancellationToken => HttpClient.PostAsJsonAsync("api/synchronization", request, cancellationToken), EmptyResponseAsync);

    public async Task DeleteTransactionAsync(string guildId, string messageId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.DeleteAsync($"api/transaction/{guildId}/{messageId}", cancellationToken), EmptyResponseAsync);

    public async Task DeleteTransactionAsync(string guildId, string messageId, string reactionId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.DeleteAsync($"api/transaction/{guildId}/{messageId}/{reactionId}", cancellationToken), EmptyResponseAsync);

    public async Task<ValidationProblemDetails?> TransferPointsAsync(TransferPointsRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/transaction/transfer", request, cancellationToken),
            async (response, cancellationToken) =>
            {
                if (response.StatusCode == HttpStatusCode.NotAcceptable) return null;
                return await DeserializeValidationErrorsAsync(response, cancellationToken);
            },
            async (response, cancellationToken) =>
            {
                if (response.StatusCode is HttpStatusCode.NotAcceptable or HttpStatusCode.BadRequest or HttpStatusCode.OK)
                    return;
                await EnsureSuccessResponseAsync(response, cancellationToken);
            }
        );
    }

    public async Task<ValidationProblemDetails?> CreateTransactionAsync(TransactionRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/transaction", request, cancellationToken),
            async (response, cancellationToken) =>
            {
                if (response.StatusCode == HttpStatusCode.NotAcceptable) return null;
                return await DeserializeValidationErrorsAsync(response, cancellationToken);
            },
            async (response, cancellationToken) =>
            {
                if (response.StatusCode is HttpStatusCode.NotAcceptable or HttpStatusCode.BadRequest or HttpStatusCode.OK)
                    return;
                await EnsureSuccessResponseAsync(response, cancellationToken);
            }
        );
    }

    public async Task<ValidationProblemDetails?> CreateTransactionAsync(AdminTransactionRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/admin/create", request, cancellationToken),
            async (response, cancellationToken) =>
            {
                if (response.StatusCode != HttpStatusCode.NotAcceptable)
                    return await DeserializeValidationErrorsAsync(response, cancellationToken);

                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Request", "NotAcceptable");
                return new ValidationProblemDetails(modelState);
            },
            async (response, cancellationToken) =>
            {
                if (response.StatusCode is HttpStatusCode.NotAcceptable or HttpStatusCode.BadRequest or HttpStatusCode.OK)
                    return;
                await EnsureSuccessResponseAsync(response, cancellationToken);
            }
        );
    }

    public async Task<bool> ExistsAnyTransactionAsync(string guildId, string userId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/transaction/{guildId}/{userId}", cancellationToken), ReadJsonAsync<bool>);

    public async Task<StatusInfo> GetStatusInfoAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag/status", cancellationToken), ReadJsonAsync<StatusInfo>);
}
