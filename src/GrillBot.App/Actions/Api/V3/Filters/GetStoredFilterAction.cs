using GrillBot.Common.Models;
using GrillBot.Core.Caching;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.App.Actions.Api.V3.Filters;

public class GetStoredFilterAction : ApiAction
{
    private readonly IDistributedCache _distributedCache;

    public GetStoredFilterAction(ApiRequestContext apiContext, IDistributedCache distributedCache) : base(apiContext)
    {
        _distributedCache = distributedCache;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var filterId = GetParameter<Guid>(0);
        var data = await _distributedCache.GetAsync<string>($"StoredFilter({filterId})");

        return string.IsNullOrEmpty(data) ?
            ApiResult.NotFound() :
            ApiResult.Ok(data);
    }
}
