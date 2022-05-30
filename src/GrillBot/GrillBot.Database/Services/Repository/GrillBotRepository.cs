using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Database.Services.Repository;

public sealed class GrillBotRepository : IDisposable
{
    private GrillBotContext Context { get; }
    private List<RepositoryBase> Repositories { get; }
    private CounterManager CounterManager { get; }

    public GrillBotRepository(GrillBotContext context, CounterManager counterManager)
    {
        Context = context;
        Repositories = new List<RepositoryBase>();
        CounterManager = counterManager;
    }

    public ChannelRepository Channel => GetOrCreateRepository<ChannelRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : RepositoryBase
    {
        var repository = Repositories.OfType<TRepository>().FirstOrDefault();

        if (repository == null)
        {
            repository = Activator.CreateInstance(typeof(TRepository), new object[] { Context, CounterManager }) as TRepository;
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

    public void RemoveCollection<TEntity>(IEnumerable<TEntity> collection) where TEntity : class
    {
        collection = collection.Where(o => o != null);

        if (!collection.Any())
            return;

        Context.Set<TEntity>().RemoveRange(collection);
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
        if (Context.Database.GetPendingMigrations().Any())
            Context.Database.Migrate();
    }

    public void Dispose()
    {
        Context?.Dispose();
        Repositories.Clear();
    }
}
