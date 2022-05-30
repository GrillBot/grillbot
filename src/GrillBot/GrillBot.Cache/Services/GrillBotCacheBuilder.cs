using GrillBot.Cache.Services.Repository;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Cache.Services;

public class GrillBotCacheBuilder
{
    private IServiceProvider ServiceProvider { get; }

    public GrillBotCacheBuilder(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public virtual GrillBotCacheRepository CreateRepository()
    {
        var options = ServiceProvider.GetRequiredService<DbContextOptions<GrillBotCacheContext>>();
        var context = new GrillBotCacheContext(options);
        var counter = ServiceProvider.GetRequiredService<CounterManager>();

        return new GrillBotCacheRepository(context, counter);
    }
}
