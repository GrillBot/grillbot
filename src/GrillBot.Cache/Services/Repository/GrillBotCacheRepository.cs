using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Cache.Services.Repository;

public sealed class GrillBotCacheRepository : RepositoryBase<GrillBotCacheContext>, IDisposable, IAsyncDisposable
{
    private List<SubRepositoryBase<GrillBotCacheContext>> Repositories { get; }

    public GrillBotCacheRepository(GrillBotCacheContext context, ICounterManager counterManager) : base(context, counterManager)
    {
        Repositories = new List<SubRepositoryBase<GrillBotCacheContext>>();
    }

    public MessageIndexRepository MessageIndexRepository => GetOrCreateRepository<MessageIndexRepository>();
    public StatisticsRepository StatisticsRepository => GetOrCreateRepository<StatisticsRepository>();
    public ProfilePictureRepository ProfilePictureRepository => GetOrCreateRepository<ProfilePictureRepository>();
    public InviteMetadataRepository InviteMetadataRepository => GetOrCreateRepository<InviteMetadataRepository>();
    public DataCacheRepository DataCache => GetOrCreateRepository<DataCacheRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : SubRepositoryBase<GrillBotCacheContext>
    {
        var repository = Repositories.OfType<TRepository>().FirstOrDefault();
        if (repository != null) return repository;

        repository = Activator.CreateInstance(typeof(TRepository), Context, CounterManager) as TRepository;
        if (repository == null)
            throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

        Repositories.Add(repository);
        return repository;
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
