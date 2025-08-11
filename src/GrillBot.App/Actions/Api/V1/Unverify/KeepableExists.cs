using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Selfunverify;
using UnverifyService;
using UnverifyService.Models.Request.Keepables;

namespace GrillBot.App.Actions.Api.V1.Unverify;

public class KeepableExists(
    ApiRequestContext apiContext,
    IServiceClientExecutor<IUnverifyServiceClient> _unverifyClient
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = (KeepableParams)Parameters[0]!;

        var request = new KeepablesListRequest
        {
            Group = parameters.Group,
            Pagination = new Core.Models.Pagination.PaginatedParams
            {
                Page = 0,
                PageSize = int.MaxValue
            }
        };

        var data = await _unverifyClient.ExecuteRequestAsync(
            async (client, ctx) => await client.GetKeepablesListAsync(request, ctx.CancellationToken),
            CancellationToken
        );

        var result = data.Data.Exists(o => o.Name.Equals(parameters.Name, StringComparison.InvariantCultureIgnoreCase));
        return ApiResult.Ok(result);
    }
}
