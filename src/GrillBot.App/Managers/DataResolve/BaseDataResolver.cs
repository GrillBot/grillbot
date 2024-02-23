using AutoMapper;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Managers.DataResolve;

public abstract class BaseDataResolver<TKey, TDiscordEntity, TDatabaseEntity, TMappedValue>
    : IDisposable where TDiscordEntity : class where TKey : notnull where TDatabaseEntity : class
{
    private readonly Dictionary<TKey, TMappedValue> _cachedData = new();
    private bool disposedValue;

    protected readonly IDiscordClient _discordClient;
    protected readonly GrillBotDatabaseBuilder _databaseBuilder;
    private readonly IMapper _mapper;

    protected BaseDataResolver(IDiscordClient discordClient, IMapper mapper, GrillBotDatabaseBuilder databaseBuilder)
    {
        _discordClient = discordClient;
        _mapper = mapper;
        _databaseBuilder = databaseBuilder;
    }

    private TMappedValue MapAndStore(TKey key, TDiscordEntity entity)
    {
        var mapped = _mapper.Map<TMappedValue>(entity);
        _cachedData[key] = mapped;

        return mapped;
    }

    private TMappedValue MapAndStore(TKey key, TDatabaseEntity entity)
    {
        var mapped = _mapper.Map<TMappedValue>(entity);
        _cachedData[key] = mapped;

        return mapped;
    }

    protected async Task<TMappedValue?> GetMappedEntityAsync(
        TKey key,
        Func<Task<TDiscordEntity?>> readDiscordEntity,
        Func<GrillBotRepository, Task<TDatabaseEntity?>> readDatabaseEntity
    )
    {
        if (_cachedData.TryGetValue(key, out var value))
            return value;

        var discordEntity = await readDiscordEntity();
        if (discordEntity is not null)
            return MapAndStore(key, discordEntity);

        await using var repository = _databaseBuilder.CreateRepository();

        var databaseEntity = await readDatabaseEntity(repository);
        return databaseEntity is null ? default : MapAndStore(key, databaseEntity);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _cachedData.Clear();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
