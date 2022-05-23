using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public sealed class GrillBotCacheRepository : IDisposable
{
    private GrillBotCacheContext Context { get; }
    private List<RepositoryBase> Repositories { get; }

    public GrillBotCacheRepository(GrillBotCacheContext context)
    {
        Context = context;
        Repositories = new();
    }

    public DirectApiRepository DirectApiRepository => GetOrCreateRepository<DirectApiRepository>();
    public MessageIndexRepository MessageIndexRepository => GetOrCreateRepository<MessageIndexRepository>();
    public StatisticsRepository StatisticsRepository => GetOrCreateRepository<StatisticsRepository>();
    public ProfilePictureRepository ProfilePictureRepository => GetOrCreateRepository<ProfilePictureRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : RepositoryBase
    {
        var repository = Repositories.OfType<TRepository>().FirstOrDefault();

        if (repository == null)
        {
            repository = Activator.CreateInstance(typeof(TRepository), new object[] { Context }) as TRepository;
            if (repository == null)
                throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

            Repositories.Add(repository);
        }

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
        collection = collection.Where(o => o != null);

        if (!collection.Any())
            return;

        Context.Set<TEntity>().RemoveRange(collection);
    }

    public async Task<int> CommitAsync()
        => await Context.SaveChangesAsync();

    public int Commit()
        => Context.SaveChanges();

    public async Task ProcessMigrationsAsync()
    {
        if ((await Context.Database.GetPendingMigrationsAsync()).Any())
            await Context.Database.MigrateAsync();
    }

    public void ProcessMigrations()
        => ProcessMigrationsAsync().Wait();

    public void Dispose()
    {
        Context?.Dispose();
        Repositories.Clear();
    }
}
