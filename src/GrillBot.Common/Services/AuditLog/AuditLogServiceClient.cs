using System.Net.Http.Json;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Core.Managers.Performance;
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
            () => HttpClient.PostAsJsonAsync("api/log", requests),
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
}
