using System.Linq;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public abstract class RepositoryBase
{
    protected GrillBotContext Context { get; }
    protected CounterManager Counter { get; }

    protected RepositoryBase(GrillBotContext context, CounterManager counter)
    {
        Context = context;
        Counter = counter;
    }

    protected IQueryable<TEntity> CreateQuery<TEntity>(IQueryableModel<TEntity> parameters, bool disableTracking = false,
        bool splitQuery = false) where TEntity : class
    {
        var query = Context.Set<TEntity>().AsQueryable();

        query = parameters.SetIncludes(query);
        query = parameters.SetQuery(query);
        query = parameters.SetSort(query);

        if (disableTracking)
            query = query.AsNoTracking();
        if (splitQuery)
            query = query.AsSplitQuery();

        return query;
    }
}
