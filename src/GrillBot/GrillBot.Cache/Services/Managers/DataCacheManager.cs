using GrillBot.Cache.Entity;

namespace GrillBot.Cache.Services.Managers;

public class DataCacheManager
{
    private GrillBotCacheBuilder CacheBuilder { get; }

    public DataCacheManager(GrillBotCacheBuilder cacheBuilder)
    {
        CacheBuilder = cacheBuilder;
    }

    public async Task SetValueAsync(string key, string value, DateTime validTo)
    {
        await using var repository = CacheBuilder.CreateRepository();

        var entity = await repository.DataCache.FindItemAsync(key, true);
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

    public async Task<string?> GetValueAsync(string key)
    {
        await using var repository = CacheBuilder.CreateRepository();

        await repository.DataCache.DeleteExpiredAsync();
        var entity = await repository.DataCache.FindItemAsync(key, disableTracking: true);
        return entity?.Value;
    }
}
