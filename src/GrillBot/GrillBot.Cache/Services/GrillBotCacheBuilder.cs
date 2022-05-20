using GrillBot.Cache.Services.Repository;
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

        return new GrillBotCacheRepository(context);
    }
}
