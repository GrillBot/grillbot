using GrillBot.Database.Services.Repository;
using Microsoft.Extensions.Caching.Memory;

namespace GrillBot.App.Managers.DataResolve;

public abstract class BaseDataResolver<TKey, TDiscordEntity, TDatabaseEntity, TMappedValue>
    where TDiscordEntity : class where TKey : notnull where TDatabaseEntity : class
{
    protected readonly IDiscordClient _discordClient;
    protected readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly IMemoryCache _memoryCache;

    protected BaseDataResolver(IDiscordClient discordClient, GrillBotDatabaseBuilder databaseBuilder,
        IMemoryCache memoryCache)
    {
        _discordClient = discordClient;
        _databaseBuilder = databaseBuilder;
        _memoryCache = memoryCache;
    }

    protected abstract TMappedValue Map(TDiscordEntity discordEntity);
    protected abstract TMappedValue Map(TDatabaseEntity entity);

    private TMappedValue MapAndStore(TKey key, TDiscordEntity entity)
    {
        var mapped = Map(entity);
        _memoryCache.Set(key, mapped, DateTimeOffset.UtcNow.AddHours(1));

        return mapped;
    }

    private TMappedValue MapAndStore(TKey key, TDatabaseEntity entity)
    {
        var mapped = Map(entity);
        _memoryCache.Set(key, mapped, DateTimeOffset.UtcNow.AddHours(1));

        return mapped;
    }

    protected async Task<TMappedValue?> GetMappedEntityAsync(
        TKey key,
        Func<Task<TDiscordEntity?>> readDiscordEntity,
        Func<GrillBotRepository, Task<TDatabaseEntity?>> readDatabaseEntity
    )
    {
        if (_memoryCache.TryGetValue<TMappedValue>(key, out var value))
            return value;

        var discordEntity = await readDiscordEntity();
        if (discordEntity is not null)
            return MapAndStore(key, discordEntity);

        await using var repository = _databaseBuilder.CreateRepository();

        var databaseEntity = await readDatabaseEntity(repository);
        return databaseEntity is null ? default : MapAndStore(key, databaseEntity);
    }
}
