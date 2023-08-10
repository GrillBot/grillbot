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

    public AuditLogServiceClient(ICounterManager counterManager, IHttpClientFactory httpClientFactory) : base(counterManager, httpClientFactory)
    {
    }

    public async Task CreateItemsAsync(List<LogRequest> requests)
        => await ProcessRequestAsync(cancellationToken => HttpClient.PostAsJsonAsync("api/logItem", requests, cancellationToken), EmptyResponseAsync);

    public async Task<DiagnosticInfo> GetDiagAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag", cancellationToken), ReadJsonAsync<DiagnosticInfo>);

    public async Task<DeleteItemResponse> DeleteItemAsync(Guid id)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.DeleteAsync($"api/logItem/{id}", cancellationToken),
            async (response, cancellationToken) => response.StatusCode == HttpStatusCode.NotFound ? default : await ReadJsonAsync<DeleteItemResponse>(response, cancellationToken),
            (response, cancellationToken) => response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound ? Task.CompletedTask : EnsureSuccessResponseAsync(response, cancellationToken)
        );
    }

    public async Task<RestResponse<PaginatedResponse<LogListItem>>> SearchItemsAsync(SearchRequest request)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync("api/logItem/search", request, cancellationToken),
            ReadRestResponseAsync<PaginatedResponse<LogListItem>>,
            (response, cancellationToken) => response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest ? Task.CompletedTask : EnsureSuccessResponseAsync(response, cancellationToken)
        );
    }

    public async Task<Detail?> DetailAsync(Guid id)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync($"api/logItem/{id}", cancellationToken),
            async (response, cancellationToken) => response.StatusCode == HttpStatusCode.NotFound ? null : await ReadJsonAsync<Detail>(response, cancellationToken),
            (response, cancellationToken) => response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound ? Task.CompletedTask : EnsureSuccessResponseAsync(response, cancellationToken)
        );
    }

    public async Task<ArchivationResult?> ProcessArchivationAsync()
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsync("api/archivation", null, cancellationToken),
            ReadJsonAsync<ArchivationResult>,
            timeout: System.Threading.Timeout.InfiniteTimeSpan
        );
    }

    public async Task<ApiStatistics> GetApiStatisticsAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/statistics/api/stats", cancellationToken), ReadJsonAsync<ApiStatistics>);

    public async Task<AuditLogStatistics> GetAuditLogStatisticsAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/statistics/auditlog", cancellationToken), ReadJsonAsync<AuditLogStatistics>);

    public async Task<AvgExecutionTimes> GetAvgTimesAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/statistics/avgtimes", cancellationToken), ReadJsonAsync<AvgExecutionTimes>);

    public async Task<List<StatisticItem>> GetInteractionStatisticsListAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/statistics/interactions/list", cancellationToken), ReadJsonAsync<List<StatisticItem>>);

    public async Task<List<UserActionCountItem>> GetUserApiStatisticsAsync(string criteria)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/statistics/api/userstats/{criteria}", cancellationToken), ReadJsonAsync<List<UserActionCountItem>>);

    public async Task<List<UserActionCountItem>> GetUserCommandStatisticsAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/statistics/interactions/userstats", cancellationToken), ReadJsonAsync<List<UserActionCountItem>>);

    public async Task<List<JobInfo>> GetJobsInfoAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/info/jobs", cancellationToken), ReadJsonAsync<List<JobInfo>>);

    public async Task<int> GetItemsCountOfGuildAsync(ulong guildId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/info/guild/{guildId}/count", cancellationToken), ReadJsonAsync<int>);

    public async Task<List<DashboardInfoRow>> GetApiDashboardAsync(string apiGroup)
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/dashboard/api/{apiGroup}", cancellationToken), ReadJsonAsync<List<DashboardInfoRow>>);

    public async Task<List<DashboardInfoRow>> GetInteractionsDashboardAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/dashboard/interactions", cancellationToken), ReadJsonAsync<List<DashboardInfoRow>>);

    public async Task<List<DashboardInfoRow>> GetJobsDashboardAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/dashboard/jobs", cancellationToken), ReadJsonAsync<List<DashboardInfoRow>>);

    public async Task<TodayAvgTimes> GetTodayAvgTimes()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/dashboard/todayavgtimes", cancellationToken), ReadJsonAsync<TodayAvgTimes>);

    public async Task<StatusInfo> GetStatusInfoAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag/status", cancellationToken), ReadJsonAsync<StatusInfo>);
}
