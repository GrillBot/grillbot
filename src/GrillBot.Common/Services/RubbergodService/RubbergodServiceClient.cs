using System.Net.Http.Json;
using GrillBot.Common.Services.RubbergodService.Models.DirectApi;
using GrillBot.Common.Services.RubbergodService.Models.Karma;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.RubbergodService;

public class RubbergodServiceClient : RestServiceBase, IRubbergodServiceClient
{
    public override string ServiceName => "RubbergodService";

    public RubbergodServiceClient(ICounterManager counterManager, IHttpClientFactory clientFactory) : base(counterManager, () => clientFactory.CreateClient("RubbergodService"))
    {
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

    public async Task InvalidatePinCacheAsync(ulong guildId, ulong channelId)
    {
        await ProcessRequestAsync(
            () => HttpClient.DeleteAsync($"api/pins/{guildId}/{channelId}"),
            _ => EmptyResult
        );
    }

    public async Task<byte[]> GetPinsAsync(ulong guildId, ulong channelId, bool markdown)
    {
        return await ProcessRequestAsync(
            () => HttpClient.GetAsync($"api/pins/{guildId}/{channelId}?markdown={markdown}"),
            response => response.Content.ReadAsByteArrayAsync()
        );
    }
}
