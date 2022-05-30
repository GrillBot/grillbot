using GrillBot.Common.Managers.Counters;

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
}
