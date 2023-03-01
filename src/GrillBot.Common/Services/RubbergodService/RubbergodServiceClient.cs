using System.Net.Http.Json;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Models.Pagination;
using GrillBot.Common.Services.Common.Models.Diagnostics;
using GrillBot.Common.Services.RubbergodService.Models.DirectApi;
using GrillBot.Common.Services.RubbergodService.Models.Karma;

namespace GrillBot.Common.Services.RubbergodService;

public class RubbergodServiceClient : RestServiceBase, IRubbergodServiceClient
{
    public override string ServiceName => "RubbergodService";

    public RubbergodServiceClient(CounterManager counterManager, IHttpClientFactory clientFactory) : base(counterManager, () => clientFactory.CreateClient("RubbergodService"))
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

    public async Task<DiagnosticInfo> GetDiagAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("api/diag"),
            response => response.Content.ReadFromJsonAsync<DiagnosticInfo>()
        ))!;
    }

    public async Task RefreshMemberAsync(ulong memberId)
    {
        await ProcessRequestAsync(
            () => HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"api/user/{memberId}")),
            _ => EmptyResult
        );
    }

    public async Task<string> SendDirectApiCommand(string service, DirectApiCommand command)
    {
        var result = await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync($"api/directApi/{service}", command),
            response => response.Content.ReadAsStringAsync()
        );

        return result;
    }

    public async Task<PaginatedResponse<UserKarma>> GetKarmaPageAsync(PaginatedParams parameters)
    {
        var query = $"Page={parameters.Page}&PageSize={parameters.PageSize}";

        var result = await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/karma?{query}"),
            response => response.Content.ReadFromJsonAsync<PaginatedResponse<UserKarma>>()
        );

        return result!;
    }

    public async Task StoreKarmaAsync(List<KarmaItem> items)
    {
        await ProcessRequestAsync(
            () => HttpClient.PostAsJsonAsync("api/karma", items),
            _ => EmptyResult
        );
    }
}
