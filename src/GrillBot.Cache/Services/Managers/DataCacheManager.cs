using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Redis.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.Cache.Services.Managers;

public class DataCacheManager
{
    private static readonly SemaphoreSlim _semaphore = new(1);
    private readonly IDistributedCache _cache;
    private readonly ICounterManager _counterManager;

    public DataCacheManager(IDistributedCache cache, ICounterManager counterManager)
    {
        _cache = cache;
        _counterManager = counterManager;
    }

    public async Task SetValueAsync<TValue>(string key, TValue value, TimeSpan? expiration)
    {
        await _semaphore.WaitAsync();

        try
        {
            using (_counterManager.Create("DataCache"))
                await _cache.SetAsync(key, value, expiration);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<TValue?> GetValueAsync<TValue>(string key)
    {
        await _semaphore.WaitAsync();

        try
        {
            using (_counterManager.Create("DataCache"))
                return await _cache.GetAsync<TValue>(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
