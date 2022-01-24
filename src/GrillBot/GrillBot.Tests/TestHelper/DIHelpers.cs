using GrillBot.Database.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace GrillBot.Tests.TestHelper
{
    public static class DIHelpers
    {
        public static ServiceProvider CreateContainer(Action<IServiceCollection> addServices = null, bool singletonContext = false)
        {
            var services = new ServiceCollection()
                .AddSingleton(TestHelpers.CreateDbOptionsBuilder().Options)
                .AddSingleton<GrillBotContextFactory, TestingGrillBotContextFactory>();

            if (singletonContext)
                services.AddSingleton(_ => TestHelpers.CreateDbContext());
            else
                services.AddTransient(_ => TestHelpers.CreateDbContext());

            addServices?.Invoke(services);
            return services.BuildServiceProvider();
        }
    }
}
