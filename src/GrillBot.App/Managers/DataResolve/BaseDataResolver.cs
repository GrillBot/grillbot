using GrillBot.Cache.Services.Managers;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Managers.DataResolve;

public abstract class BaseDataResolver<TDiscordEntity, TDatabaseEntity, TMappedValue>
    where TDiscordEntity : class where TDatabaseEntity : class
{
    protected readonly IDiscordClient _discordClient;
    protected readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly DataCacheManager _dataCache;

    protected BaseDataResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        DataCacheManager dataCache)
    {
        _discordClient = discordClient;
        _databaseBuilder = databaseBuilder;
        _dataCache = dataCache;
    }

    protected abstract TMappedValue Map(TDiscordEntity discordEntity);
    protected abstract TMappedValue Map(TDatabaseEntity entity);

    private async Task<TMappedValue> MapAndStoreAsync(string key, TDiscordEntity entity)
    {
        var mapped = Map(entity);
        await _dataCache.SetValueAsync(key, mapped, TimeSpan.FromHours(1));

        return mapped;
    }

    private async Task<TMappedValue> MapAndStoreAsync(string key, TDatabaseEntity entity)
    {
        var mapped = Map(entity);
        await _dataCache.SetValueAsync(key, mapped, TimeSpan.FromHours(1));

        return mapped;
    }

    protected async Task<TMappedValue?> GetMappedEntityAsync(
        string key,
        Func<Task<TDiscordEntity?>> readDiscordEntity,
        Func<GrillBotRepository, Task<TDatabaseEntity?>> readDatabaseEntity
    )
    {
        var value = await _dataCache.GetValueAsync<TMappedValue>(key);
        if (value is not null)
            return value;

        var discordEntity = await readDiscordEntity();
        if (discordEntity is not null)
            return await MapAndStoreAsync(key, discordEntity);

        await using var repository = _databaseBuilder.CreateRepository();

        var databaseEntity = await readDatabaseEntity(repository);
        return databaseEntity is null ? default : await MapAndStoreAsync(key, databaseEntity);
    }
}
