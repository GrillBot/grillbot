using System.Net.Http.Json;
using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Common.Services.KachnaOnline;

public class KachnaOnlineClient : RestServiceBase, IKachnaOnlineClient
{
    public override string ServiceName => "KachnaOnline";

    public KachnaOnlineClient(IHttpClientFactory httpClientFactory, ICounterManager counterManager) : base(counterManager, () => httpClientFactory.CreateClient("KachnaOnline"))
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
