using GrillBot.Core.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.Cache.Services.Managers;

public class DataCacheManager
{
    private static readonly SemaphoreSlim _semaphore = new(1);
    private readonly IDistributedCache _cache;

    public DataCacheManager(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SetValueAsync<TValue>(string key, TValue value, TimeSpan? expiration)
    {
        await _semaphore.WaitAsync();

        try
        {
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
            return await _cache.GetAsync<TValue>(key);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
