using System.Net;
using System.Net.Http.Json;
using GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;
using GrillBot.Common.Services.AuditLog.Models.Request.Search;
using GrillBot.Common.Services.AuditLog.Models.Response;
using GrillBot.Common.Services.AuditLog.Models.Response.Detail;
using GrillBot.Common.Services.AuditLog.Models.Response.Info;
using GrillBot.Common.Services.AuditLog.Models.Response.Info.Dashboard;
using GrillBot.Common.Services.AuditLog.Models.Response.Search;
using GrillBot.Common.Services.AuditLog.Models.Response.Statistics;
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

    public async Task<ArchivationResult?> ProcessArchivationAsync()
    {
        return await ProcessRequestAsync(
            () => HttpClient.PostAsync("api/archivation", null),
            async response => response.StatusCode == HttpStatusCode.NoContent ? null : await response.Content.ReadFromJsonAsync<ArchivationResult>()
        );
    }

    public async Task<ApiStatistics> GetApiStatisticsAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/statistics/api/stats"),
            response => response.Content.ReadFromJsonAsync<ApiStatistics>()
        ))!;
    }

    public async Task<AuditLogStatistics> GetAuditLogStatisticsAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/statistics/auditlog"),
            response => response.Content.ReadFromJsonAsync<AuditLogStatistics>()
        ))!;
    }

    public async Task<AvgExecutionTimes> GetAvgTimesAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/statistics/avgtimes"),
            response => response.Content.ReadFromJsonAsync<AvgExecutionTimes>()
        ))!;
    }

    public async Task<List<StatisticItem>> GetInteractionStatisticsListAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/statistics/interactions/list"),
            response => response.Content.ReadFromJsonAsync<List<StatisticItem>>()
        ))!;
    }

    public async Task<List<UserActionCountItem>> GetUserApiStatisticsAsync(string criteria)
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/statistics/api/userstats/{criteria}"),
            response => response.Content.ReadFromJsonAsync<List<UserActionCountItem>>()
        ))!;
    }

    public async Task<List<UserActionCountItem>> GetUserCommandStatisticsAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/statistics/interactions/userstats"),
            response => response.Content.ReadFromJsonAsync<List<UserActionCountItem>>()
        ))!;
    }

    public async Task<List<JobInfo>> GetJobsInfoAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/info/jobs"),
            response => response.Content.ReadFromJsonAsync<List<JobInfo>>()
        ))!;
    }

    public async Task<DashboardInfo> GetDashboardInfoAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/info/dashboard"),
            response => response.Content.ReadFromJsonAsync<DashboardInfo>()
        ))!;
    }
}
