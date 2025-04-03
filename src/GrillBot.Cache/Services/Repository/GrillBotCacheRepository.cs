using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;

#pragma warning disable S3604 // Member initializer values should not be redundant
namespace GrillBot.Cache.Services.Repository;

public class GrillBotCacheRepository(
    GrillBotCacheContext context,
    ICounterManager counterManager
) : RepositoryBase<GrillBotCacheContext>(context, counterManager)
{
    private readonly List<SubRepositoryBase<GrillBotCacheContext>> _repositories = [];

    public MessageIndexRepository MessageIndexRepository => GetOrCreateRepository<MessageIndexRepository>();
    public StatisticsRepository StatisticsRepository => GetOrCreateRepository<StatisticsRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : SubRepositoryBase<GrillBotCacheContext>
    {
        var repository = _repositories.OfType<TRepository>().FirstOrDefault();
        if (repository != null) return repository;

        repository = Activator.CreateInstance(typeof(TRepository), DbContext, CounterManager) as TRepository;
        if (repository == null)
            throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

        _repositories.Add(repository);
        return repository;
    }

    protected override void DisposeInternal()
    {
        _repositories.Clear();
    }
}
