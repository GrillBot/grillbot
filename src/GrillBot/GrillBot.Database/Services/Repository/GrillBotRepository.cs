using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Database.Services.Repository;

public sealed class GrillBotRepository : IDisposable, IAsyncDisposable
{
    private GrillBotContext Context { get; }
    private List<RepositoryBase> Repositories { get; } = new();
    private CounterManager CounterManager { get; }

    public GrillBotRepository(GrillBotContext context, CounterManager counterManager)
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
    public AuditLogRepository AuditLog => GetOrCreateRepository<AuditLogRepository>();
    public EmoteRepository Emote => GetOrCreateRepository<EmoteRepository>();
    public SearchingRepository Searching => GetOrCreateRepository<SearchingRepository>();
    public SelfUnverifyRepository SelfUnverify => GetOrCreateRepository<SelfUnverifyRepository>();
    public SuggestionRepository Suggestion => GetOrCreateRepository<SuggestionRepository>();
    public PermissionsRepository Permissions => GetOrCreateRepository<PermissionsRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : RepositoryBase
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
        using (CounterManager.Create("Database"))
        {
            return await Context.SaveChangesAsync();
        }
    }

    public void ProcessMigrations()
    {
        using (CounterManager.Create("Database"))
        {
            if (Context.Database.GetPendingMigrations().Any())
                Context.Database.Migrate();
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
