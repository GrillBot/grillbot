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

    public PointsServiceClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, () => httpClientFactory.CreateClient("PointsService"))
    {
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            await ProcessRequestAsync(
                () => HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "health")),
                _ => EmptyResult
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<RestResponse<PaginatedResponse<TransactionItem>>> GetTransactionListAsync(AdminListRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/admin/list", request),
            async response =>
            {
                var validationError = await DesrializeValidationErrorsAsync(response);
                return validationError is not null
                    ? new RestResponse<PaginatedResponse<TransactionItem>>(validationError)
                    : new RestResponse<PaginatedResponse<TransactionItem>>(await response.Content.ReadFromJsonAsync<PaginatedResponse<TransactionItem>>());
            },
            async response =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task<RestResponse<List<PointsChartItem>>> GetChartDataAsync(AdminListRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/chart", request),
            async response =>
            {
                var validationError = await DesrializeValidationErrorsAsync(response);
                return validationError is not null
                    ? new RestResponse<List<PointsChartItem>>(validationError)
                    : new RestResponse<List<PointsChartItem>>(await response.Content.ReadFromJsonAsync<List<PointsChartItem>>());
            },
            async response =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/diag"),
            response => response.Content.ReadFromJsonAsync<DiagnosticInfo>()
        ))!;
    }

    public async Task<RestResponse<List<BoardItem>>> GetLeaderboardAsync(string guildId, int skip, int count, bool simple)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/leaderboard/{guildId}?skip={skip}&count={count}&simple={(simple ? "true" : "false")}"),
            async response =>
            {
                var validationError = await DesrializeValidationErrorsAsync(response);
                return validationError is not null
                    ? new RestResponse<List<BoardItem>>(validationError)
                    : new RestResponse<List<BoardItem>>(await response.Content.ReadFromJsonAsync<List<BoardItem>>());
            },
            async response =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task<int> GetLeaderboardCountAsync(string guildId)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/leaderboard/{guildId}/count"),
            response => response.Content.ReadFromJsonAsync<int>()
        );
    }

    public async Task<MergeResult?> MergeTransctionsAsync()
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsync("api/merge", null),
            ReadJsonAsync<MergeResult>
        );
    }

    public async Task<PointsStatus> GetStatusOfPointsAsync(string guildId, string userId)
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/status/{guildId}/{userId}"),
            response => response.Content.ReadFromJsonAsync<PointsStatus>()
        ))!;
    }

    public async Task<PointsStatus> GetStatusOfExpiredPointsAsync(string guildId, string userId)
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/status/{guildId}/{userId}/expired"),
            response => response.Content.ReadFromJsonAsync<PointsStatus>()
        ))!;
    }

    public async Task<ImagePointsStatus> GetImagePointsStatusAsync(string guildId, string userId)
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/status/{guildId}/{userId}/image"),
            response => response.Content.ReadFromJsonAsync<ImagePointsStatus>()
        ))!;
    }

    public async Task ProcessSynchronizationAsync(SynchronizationRequest request)
    {
        await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/synchronization", request),
            _ => EmptyResult
        );
    }

    public async Task<ValidationProblemDetails?> CreateTransactionAsync(TransactionRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/transaction", request),
            async response =>
            {
                if (response.StatusCode == HttpStatusCode.NotAcceptable) return null;
                return await DesrializeValidationErrorsAsync(response);
            },
            async response =>
            {
                if (response.StatusCode is HttpStatusCode.NotAcceptable or HttpStatusCode.BadRequest or HttpStatusCode.OK)
                    return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task DeleteTransactionAsync(string guildId, string messageId)
    {
        await ProcessRequestAsync(
            () => HttpClient.DeleteAsync($"api/transaction/{guildId}/{messageId}"),
            _ => EmptyResult
        );
    }

    public async Task DeleteTransactionAsync(string guildId, string messageId, string reactionId)
    {
        await ProcessRequestAsync(
            () => HttpClient.DeleteAsync($"api/transaction/{guildId}/{messageId}/{reactionId}"),
            _ => EmptyResult
        );
    }

    public async Task<ValidationProblemDetails?> TransferPointsAsync(TransferPointsRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/transaction/transfer", request),
            async response =>
            {
                if (response.StatusCode == HttpStatusCode.NotAcceptable) return null;
                return await DesrializeValidationErrorsAsync(response);
            },
            async response =>
            {
                if (response.StatusCode is HttpStatusCode.NotAcceptable or HttpStatusCode.BadRequest or HttpStatusCode.OK)
                    return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task<ValidationProblemDetails?> CreateTransactionAsync(AdminTransactionRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/admin/create", request),
            async response =>
            {
                if (response.StatusCode != HttpStatusCode.NotAcceptable)
                    return await DesrializeValidationErrorsAsync(response);

                var modelState = new ModelStateDictionary();
                modelState.AddModelError("Request", "NotAcceptable");
                return new ValidationProblemDetails(modelState);
            },
            async response =>
            {
                if (response.StatusCode is HttpStatusCode.NotAcceptable or HttpStatusCode.BadRequest or HttpStatusCode.OK)
                    return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task<bool> ExistsAnyTransactionAsync(string guildId, string userId)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/transaction/{guildId}/{userId}"),
            response => response.Content.ReadFromJsonAsync<bool>()
        );
    }
}
