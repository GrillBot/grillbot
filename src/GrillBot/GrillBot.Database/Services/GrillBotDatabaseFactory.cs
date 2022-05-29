using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Database.Services;

public class GrillBotDatabaseFactory
{
    protected IServiceProvider ServiceProvider { get; }

    public GrillBotDatabaseFactory(IServiceProvider provider)
    {
        ServiceProvider = provider;
    }

    public virtual GrillBotContext Create()
    {
        var options = ServiceProvider.GetRequiredService<DbContextOptions>();
        return new GrillBotContext(options);
    }

    public virtual GrillBotRepository CreateRepository()
    {
        var options = ServiceProvider.GetRequiredService<DbContextOptions>();
        var context = new GrillBotContext(options);

        return new GrillBotRepository(context);
    }
}
