using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Cache.Services.Repository;

public sealed class GrillBotCacheRepository : IDisposable, IAsyncDisposable
{
    private GrillBotCacheContext Context { get; }
    private ICounterManager CounterManager { get; }
    private List<RepositoryBase<GrillBotCacheContext>> Repositories { get; }

    public GrillBotCacheRepository(GrillBotCacheContext context, ICounterManager counterManager)
    {
        Context = context;
        CounterManager = counterManager;
        Repositories = new List<RepositoryBase<GrillBotCacheContext>>();
    }

    public MessageIndexRepository MessageIndexRepository => GetOrCreateRepository<MessageIndexRepository>();
    public StatisticsRepository StatisticsRepository => GetOrCreateRepository<StatisticsRepository>();
    public ProfilePictureRepository ProfilePictureRepository => GetOrCreateRepository<ProfilePictureRepository>();
    public InviteMetadataRepository InviteMetadataRepository => GetOrCreateRepository<InviteMetadataRepository>();
    public DataCacheRepository DataCache => GetOrCreateRepository<DataCacheRepository>();
    public EmoteSuggestionRepository EmoteSuggestion => GetOrCreateRepository<EmoteSuggestionRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : RepositoryBase<GrillBotCacheContext>
    {
        var repository = Repositories.OfType<TRepository>().FirstOrDefault();
        if (repository != null) return repository;

        repository = Activator.CreateInstance(typeof(TRepository), Context, CounterManager) as TRepository;
        if (repository == null)
            throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

        Repositories.Add(repository);
        return repository;
    }

    public Task AddAsync<TEntity>(TEntity entity) where TEntity : class
        => Context.Set<TEntity>().AddAsync(entity).AsTask();

    public Task AddRangeAsync<TEntity>(IEnumerable<TEntity> collection) where TEntity : class
        => Context.Set<TEntity>().AddRangeAsync(collection);

    public void Remove<TEntity>(TEntity entity) where TEntity : class
        => Context.Set<TEntity>().Remove(entity);

    public void RemoveCollection<TEntity>(IEnumerable<TEntity> collection) where TEntity : class
    {
        var enumerable = collection as List<TEntity> ?? collection.ToList();
        if (enumerable.Count == 0)
            return;

        Context.Set<TEntity>().RemoveRange(enumerable);
    }

    public async Task CommitAsync()
    {
        using (CounterManager.Create("Cache.Commit"))
        {
            await Context.SaveChangesAsync();
        }
    }

    public void Dispose()
    {
        Context.Dispose();
        Repositories.Clear();
    }

    public ValueTask DisposeAsync()
    {
        Repositories.Clear();
        return Context.DisposeAsync();
    }
}
