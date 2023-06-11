using System.Net;
using System.Net.Http.Json;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Common.Services.AuditLog.Models.Request.Search;
using GrillBot.Common.Services.AuditLog.Models.Response;
using GrillBot.Common.Services.AuditLog.Models.Response.Detail;
using GrillBot.Common.Services.AuditLog.Models.Response.Search;
using GrillBot.Common.Services.Common;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.AuditLog;

public class AuditLogServiceClient : RestServiceBase, IAuditLogServiceClient
{
    public override string ServiceName => "AuditLog";

    public AuditLogServiceClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, () => httpClientFactory.CreateClient("AuditLog"))
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

    public async Task CreateItemsAsync(List<LogRequest> requests)
    {
        await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/logItem", requests),
            _ => EmptyResult
        );
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/diag"),
            response => response.Content.ReadFromJsonAsync<DiagnosticInfo>()
        ))!;
    }

    public async Task<DeleteItemResponse> DeleteItemAsync(Guid id)
    {
        return (await ProcessRequestAsync(
                () => HttpClient.DeleteAsync($"api/logItem/{id}"),
                response => response.Content.ReadFromJsonAsync<DeleteItemResponse>(),
                response => response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound ? Task.CompletedTask : EnsureSuccessResponseAsync(response))
            )!;
    }

    public async Task<RestResponse<PaginatedResponse<LogListItem>>> SearchItemsAsync(SearchRequest request)
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/logItem/search", request),
            async response =>
            {
                var validationError = await DesrializeValidationErrorsAsync(response);
                return validationError is not null
                    ? new RestResponse<PaginatedResponse<LogListItem>>(validationError)
                    : new RestResponse<PaginatedResponse<LogListItem>>(await response.Content.ReadFromJsonAsync<PaginatedResponse<LogListItem>>());
            },
            async response =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest) return;
                await EnsureSuccessResponseAsync(response);
            }
        );
    }

    public async Task<Detail?> DetailAsync(Guid id)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/logItem/{id}"),
            async response =>
            {
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<Detail>();
                return null;
            },
            response =>
            {
                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound)
                    return Task.CompletedTask;
                return EnsureSuccessResponseAsync(response);
            }
        );
    }
}
