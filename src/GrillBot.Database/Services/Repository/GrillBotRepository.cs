using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Database.Services.Repository;

public sealed class GrillBotRepository : IDisposable, IAsyncDisposable
{
    private GrillBotContext Context { get; set; }
    private List<RepositoryBase<GrillBotContext>> Repositories { get; set; } = new();
    private ICounterManager CounterManager { get; }

    public GrillBotRepository(GrillBotContext context, ICounterManager counterManager)
    {
        Context = context;
        CounterManager = counterManager;
    }

    public ChannelRepository Channel => GetOrCreateRepository<ChannelRepository>();
    public UserRepository User => GetOrCreateRepository<UserRepository>();
    public UnverifyRepository Unverify => GetOrCreateRepository<UnverifyRepository>();
    public RemindRepository Remind => GetOrCreateRepository<RemindRepository>();
    public GuildRepository Guild => GetOrCreateRepository<GuildRepository>();
    public GuildUserRepository GuildUser => GetOrCreateRepository<GuildUserRepository>();
    public InviteRepository Invite => GetOrCreateRepository<InviteRepository>();
    public EmoteRepository Emote => GetOrCreateRepository<EmoteRepository>();
    public SearchingRepository Searching => GetOrCreateRepository<SearchingRepository>();
    public SelfUnverifyRepository SelfUnverify => GetOrCreateRepository<SelfUnverifyRepository>();
    public AutoReplyRepository AutoReply => GetOrCreateRepository<AutoReplyRepository>();
    public StatisticsRepository Statistics => GetOrCreateRepository<StatisticsRepository>();
    public EmoteSuggestionRepository EmoteSuggestion => GetOrCreateRepository<EmoteSuggestionRepository>();
    public ApiClientRepository ApiClientRepository => GetOrCreateRepository<ApiClientRepository>();
    public NicknameRepository Nickname => GetOrCreateRepository<NicknameRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : RepositoryBase<GrillBotContext>
    {
        var repository = Repositories.OfType<TRepository>().FirstOrDefault();
        if (repository != null)
            return repository;

        repository = Activator.CreateInstance(typeof(TRepository), Context, CounterManager) as TRepository;
        if (repository == null)
            throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

        Repositories.Add(repository);

        return repository;
    }

    public Task AddAsync<TEntity>(TEntity entity) where TEntity : class
        => Context.Set<TEntity>().AddAsync(entity).AsTask();

    public Task AddCollectionAsync<TEntity>(IEnumerable<TEntity> collection) where TEntity : class
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

    public async Task<int> CommitAsync()
    {
        using (CounterManager.Create("Database.Commit"))
        {
            return await Context.SaveChangesAsync();
        }
    }

    public void Dispose()
    {
        Context.ChangeTracker.Clear();
        Context.Dispose();
        Context = null!;

        Repositories.Clear();
        Repositories = null!;
    }

    public async ValueTask DisposeAsync()
    {
        Repositories.Clear();
        Repositories = null!;

        Context.ChangeTracker.Clear();
        await Context.DisposeAsync();
        Context = null!;
    }
}
