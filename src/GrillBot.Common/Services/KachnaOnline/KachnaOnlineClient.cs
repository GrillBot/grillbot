using GrillBot.Common.Services.KachnaOnline.Models;
using GrillBot.Core.Services.Common;
using GrillBot.Core.Services.Common.Extensions;

namespace GrillBot.Common.Services.KachnaOnline;

public class KachnaOnlineClient : RestServiceBase, IKachnaOnlineClient
{
    public override string ServiceName => "KachnaOnline";

    public KachnaOnlineClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public async Task<DuckState> GetCurrentStateAsync()
    {
        return (await ProcessRequestAsync<DuckState>(
            () => HttpMethod.Get.ToRequest("states/current"),
            TimeSpan.FromSeconds(10)
        ))!;
    }

    public async Task<DuckState?> GetNextStateAsync(Enums.DuckState stateType)
    {
        try 
        {
            return await ProcessRequestAsync<DuckState>(
                () => HttpMethod.Get.ToRequest($"states/next?type={stateType}"),
                TimeSpan.FromSeconds(10)
            );
        }
        catch (ClientNotFoundException)
        {
            // 404 is a valid response here saying that no such state is planned
            return null;
        }
    }
}
