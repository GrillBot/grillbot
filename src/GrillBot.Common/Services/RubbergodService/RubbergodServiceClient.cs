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

    public RubbergodServiceClient(ICounterManager counterManager, IHttpClientFactory clientFactory) : base(counterManager, clientFactory)
    {
    }

    public async Task<DiagnosticInfo> GetDiagAsync()
        => await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync("api/diag", cancellationToken), ReadJsonAsync<DiagnosticInfo>);

    public async Task<string> SendDirectApiCommand(string service, DirectApiCommand command)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.PostAsJsonAsync($"api/directApi/{service}", command, cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsStringAsync(cancellationToken: cancellationToken)!
        );
    }

    public async Task<PaginatedResponse<UserKarma>> GetKarmaPageAsync(PaginatedParams parameters)
    {
        var query = $"Page={parameters.Page}&PageSize={parameters.PageSize}";
        return await ProcessRequestAsync(cancellationToken => HttpClient.GetAsync($"api/karma?{query}", cancellationToken), ReadJsonAsync<PaginatedResponse<UserKarma>>);
    }

    public async Task StoreKarmaAsync(List<KarmaItem> items)
        => await ProcessRequestAsync(cancellationToken => HttpClient.PostAsJsonAsync("api/karma", items, cancellationToken), EmptyResponseAsync);

    public async Task InvalidatePinCacheAsync(ulong guildId, ulong channelId)
        => await ProcessRequestAsync(cancellationToken => HttpClient.DeleteAsync($"api/pins/{guildId}/{channelId}", cancellationToken), EmptyResponseAsync);

    public async Task<byte[]> GetPinsAsync(ulong guildId, ulong channelId, bool markdown)
    {
        return await ProcessRequestAsync(
            cancellationToken => HttpClient.GetAsync($"api/pins/{guildId}/{channelId}?markdown={markdown}", cancellationToken),
            (response, cancellationToken) => response.Content.ReadAsByteArrayAsync(cancellationToken: cancellationToken)!
        );
    }
}
