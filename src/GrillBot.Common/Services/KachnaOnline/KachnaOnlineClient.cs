using System.Net.Http.Json;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Common.Services.KachnaOnline;

public class KachnaOnlineClient : RestServiceBase, IKachnaOnlineClient
{
    protected override string ServiceName => "KachnaOnline";

    public KachnaOnlineClient(IHttpClientFactory httpClientFactory, CounterManager counterManager) : base(counterManager, () => httpClientFactory.CreateClient("KachnaOnline"))
    {
    }


    public async Task<DuckState> GetCurrentStateAsync()
    {
        return (await ProcessRequestAsync(
            () => HttpClient.GetAsync("states/current"),
            response => response.Content.ReadFromJsonAsync<DuckState>()
        ))!;
    }
}
