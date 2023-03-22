using GrillBot.Cache.Services.Repository;
using GrillBot.Core.Managers.Performance;
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

    public GrillBotCacheRepository CreateRepository()
    {
        var options = ServiceProvider.GetRequiredService<DbContextOptions<GrillBotCacheContext>>();
        var context = new GrillBotCacheContext(options);
        var counter = ServiceProvider.GetRequiredService<ICounterManager>();

        return new GrillBotCacheRepository(context, counter);
    }
}
