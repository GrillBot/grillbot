using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Database.Services
{
    public class GrillBotContextFactory
    {
        private IServiceProvider ServiceProvider { get; }

        public GrillBotContextFactory(IServiceProvider provider)
        {
            ServiceProvider = provider;
        }

        public GrillBotContext Create()
        {
            var options = ServiceProvider.GetRequiredService<DbContextOptions>();
            return new GrillBotContext(options);
        }
    }
}
