using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Extensions;

namespace GrillBot.Common.Services.KachnaOnline;

public class KachnaOnlineClient : RestServiceBase, IKachnaOnlineClient
{
    public override string ServiceName => "KachnaOnline";

    public KachnaOnlineClient(IHttpClientFactory httpClientFactory, ICounterManager counterManager) : base(counterManager, httpClientFactory)
    {
    }

    public async Task<DuckState> GetCurrentStateAsync()
    {
        return (await ProcessRequestAsync<DuckState>(
            () => HttpMethod.Get.ToRequest("states/current"),
            TimeSpan.FromSeconds(10)
        ))!;
    }
}
