using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using UnverifyService;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class RemoveKeepables(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var group = (string)Parameters[0]!;
        var name = (string?)Parameters[1];

        await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.DeleteKeepablesAsync(group, name, CancellationToken),
            CancellationToken
        );

        return ApiResult.Ok();
    }
}
