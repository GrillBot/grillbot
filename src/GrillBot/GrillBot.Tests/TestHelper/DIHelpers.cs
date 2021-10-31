using GrillBot.Database.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Tests.TestHelper
{
    public static class DIHelpers
    {
        public static ServiceProvider CreateContainer(Action<IServiceCollection> addServices = null)
        {
            var services = new ServiceCollection()
                .AddSingleton(TestHelpers.CreateDbOptionsBuilder().Options)
                .AddTransient(_ => TestHelpers.CreateDbContext())
                .AddSingleton<GrillBotContextFactory, TestingGrillBotContextFactory>();

            addServices?.Invoke(services);
            return services.BuildServiceProvider();
        }
    }
}
