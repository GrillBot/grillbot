using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.SearchingService;

namespace GrillBot.App.Actions.Api.V1.Searching;

public class RemoveSearches : ApiAction
{
    private readonly ISearchingServiceClient _searchingService;

    public RemoveSearches(ApiRequestContext apiContext, ISearchingServiceClient searchingService) : base(apiContext)
    {
        _searchingService = searchingService;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var ids = (long[])Parameters[0]!;

        var requests = ids.Select(_searchingService.RemoveSearchingAsync);
        await Task.WhenAll(requests);

        return ApiResult.Ok();
    }
}
