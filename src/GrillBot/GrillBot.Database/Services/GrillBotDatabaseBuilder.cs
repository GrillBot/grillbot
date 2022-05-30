using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Database.Services;

public class GrillBotDatabaseBuilder
{
    protected IServiceProvider ServiceProvider { get; }

    public GrillBotDatabaseBuilder(IServiceProvider provider)
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
        var counter = ServiceProvider.GetRequiredService<CounterManager>();

        return new GrillBotRepository(context, counter);
    }
}
