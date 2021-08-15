using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Database.Services
{
    public class GrillBotContextFactory
    {
        protected IServiceProvider ServiceProvider { get; }

        public GrillBotContextFactory(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        public virtual GrillBotContext Create()
        {
            var options = ServiceProvider.GetRequiredService<DbContextOptions>();
            return new GrillBotContext(options);
        }
    }
}
