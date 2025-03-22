using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Redis.Extensions;
using GrillBot.Data.Models.API.Filters;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.App.Actions.Api.V3.Filters;

public class StoreFilterAction : ApiAction
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConfiguration _configuration;

    public StoreFilterAction(
        ApiRequestContext apiContext,
        IDistributedCache distributedCache,
        IConfiguration configuration
    ) : base(apiContext)
    {
        _distributedCache = distributedCache;
        _configuration = configuration.GetSection("WebAdmin:Filters");
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var input = GetParameter<StoredFilterInput>(0);
        var filterId = Guid.NewGuid();
        var filterKey = $"StoredFilter({filterId})";
        var expirationTime = _configuration.GetValue<TimeSpan>("ExpirationTime");
        var expiresAt = DateTime.UtcNow.Add(expirationTime);

        await _distributedCache.SetAsync(filterKey, input.FilterData, expirationTime);

        var result = new StoredFilterInfo(filterId, expiresAt);
        return ApiResult.Ok(result);
    }
}
