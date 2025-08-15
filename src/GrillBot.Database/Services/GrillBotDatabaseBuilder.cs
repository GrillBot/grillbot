using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services;

public class GrillBotDatabaseBuilder
{
    private ICounterManager CounterManager { get; }
    private DbContextOptions Options { get; }

    public GrillBotDatabaseBuilder(ICounterManager counterManager, DbContextOptions options)
    {
        CounterManager = counterManager;
        Options = options;
    }

    public GrillBotContext CreateContext()
        => new(Options);

    public GrillBotRepository CreateRepository()
    {
        var context = CreateContext();
        return new GrillBotRepository(context, CounterManager);
    }
}
