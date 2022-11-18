using GrillBot.Cache.Entity;

namespace GrillBot.Cache.Services.Managers;

public class DataCacheManager
{
    private GrillBotCacheBuilder CacheBuilder { get; }
    private static SemaphoreSlim Semaphore { get; }

    static DataCacheManager()
    {
        Semaphore = new SemaphoreSlim(1);
    }

    public DataCacheManager(GrillBotCacheBuilder cacheBuilder)
    {
        CacheBuilder = cacheBuilder;
    }

    public async Task SetValueAsync(string key, string value, DateTime validTo)
    {
        await Semaphore.WaitAsync();
        try
        {
            await using var repository = CacheBuilder.CreateRepository();

            var entity = await repository.DataCache.FindItemAsync(key);
            if (entity == null)
            {
                entity = new DataCacheItem { Key = key };
                await repository.AddAsync(entity);
            }

            entity.Value = value;
            entity.ValidTo = validTo;

            await repository.CommitAsync();
            await repository.DataCache.DeleteExpiredAsync();
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<string?> GetValueAsync(string key)
    {
        await Semaphore.WaitAsync();
        try
        {
            await using var repository = CacheBuilder.CreateRepository();

            await repository.DataCache.DeleteExpiredAsync();
            var entity = await repository.DataCache.FindItemAsync(key, true);
            return entity?.Value;
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
