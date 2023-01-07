using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services;

public class GrillBotDatabaseBuilder
{
    private CounterManager CounterManager { get; }
    private DbContextOptions Options { get; }

    public GrillBotDatabaseBuilder(CounterManager counterManager, DbContextOptions options)
    {
        CounterManager = counterManager;
        Options = options;
    }

    public virtual GrillBotRepository CreateRepository()
    {
        var context = new GrillBotContext(Options);
        return new GrillBotRepository(context, CounterManager);
    }
}
