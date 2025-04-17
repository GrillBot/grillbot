using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.SearchingService;

namespace GrillBot.App.Actions.Api.V1.Searching;

public class RemoveSearches(
    ApiRequestContext apiContext,
    IServiceClientExecutor<ISearchingServiceClient> _searchingService
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        foreach (var id in GetParameter<long[]>(0))
            await _searchingService.ExecuteRequestAsync((c, ctx) => c.RemoveSearchingAsync(id, ctx.AuthorizationToken, ctx.CancellationToken));

        return ApiResult.Ok();
    }
}
