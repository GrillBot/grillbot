using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Tests.TestHelper
{
    public static class DIHelpers
    {
        public static ServiceProvider CreateContainer()
        {
            var services = new ServiceCollection()
                .AddSingleton(TestHelpers.CreateDbOptionsBuilder().Options)
                .AddTransient(_ => TestHelpers.CreateDbContext())
                .AddSingleton<GrillBotContextFactory, TestingGrillBotContextFactory>();

            return services.BuildServiceProvider();
        }
    }
}
