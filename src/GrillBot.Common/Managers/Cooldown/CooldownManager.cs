using GrillBot.Core.Caching;
using Microsoft.Extensions.Caching.Distributed;

namespace GrillBot.Common.Managers.Cooldown;

public class CooldownManager
{
    private readonly IDistributedCache _cache;

    public CooldownManager(IDistributedCache cache)
    {
        _cache = cache;
    }

    private readonly SemaphoreSlim _semaphore = new(1);

    public async Task SetCooldownAsync(string id, CooldownType type, int maxCount, DateTime until)
    {
        await _semaphore.WaitAsync();

        try
        {
            var key = CreateKey(id, type);

            var activeCooldown = await _cache.GetAsync<CooldownItem>(key);
            if (activeCooldown is null)
                activeCooldown = CreateItem(1, maxCount, until);
            else
                activeCooldown = CreateItem(activeCooldown.Used + 1, maxCount, until);

            await _cache.SetAsync(key, activeCooldown, null);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<TimeSpan?> GetRemainingCooldownAsync(string id, CooldownType type)
    {
        await _semaphore.WaitAsync();

        try
        {
            var key = CreateKey(id, type);
            var item = await _cache.GetAsync<CooldownItem>(key);

            if (item is null)
                return null;

            if (item.Until is null || item.Until.Value < DateTime.Now)
            {
                await _cache.RemoveAsync(key);
                return null;
            }

            return item.Until - DateTime.Now;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task DecreaseCooldownAsync(string id, CooldownType type, DateTime until)
    {
        await _semaphore.WaitAsync();

        try
        {
            var key = CreateKey(id, type);
            var item = await _cache.GetAsync<CooldownItem>(key);

            if (item is null)
                return;

            item = CreateItem(item.Used - 1, item.Max, until);
            await _cache.SetAsync(key, item, null);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> RemoveCooldownIfExpired(string id, CooldownType type)
    {
        await _semaphore.WaitAsync();

        try
        {
            var key = CreateKey(id, type);
            var item = await _cache.GetAsync<CooldownItem>(key);

            if (item is null || item.Until is null || item.Until.Value >= DateTime.Now)
                return false;

            await _cache.RemoveAsync(key);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string CreateKey(string id, CooldownType type)
        => $"Cooldown-{type}-{id}";

    private static CooldownItem CreateItem(int used, int max, DateTime until)
    {
        return new CooldownItem
        {
            Used = used,
            Until = used >= max ? until : null,
            Max = max
        };
    }
}
