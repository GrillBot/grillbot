using GrillBot.App;
using GrillBot.Data.Models.AuditLog;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GrillBot.Cache.Services;
using GrillBot.Database.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DiHelper
{
    public static IServiceProvider CreateEmptyProvider()
    {
        return new ServiceCollection()
            .AddScoped<ApiRequest>()
            .BuildServiceProvider();
    }

    public static IServiceProvider CreateInitializedProvider()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var startup = new Startup(configuration);
        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton(TestServices.TestingEnvironment.Value);

        startup.ConfigureServices(services);
        
        var dbBuilder = services.FirstOrDefault(o => o.ServiceType == typeof(GrillBotDatabaseBuilder));
        services.Remove(dbBuilder);
        services.AddSingleton<GrillBotDatabaseBuilder>(TestServices.DatabaseBuilder.Value);

        var cacheBuilder = services.FirstOrDefault(o => o.ServiceType == typeof(GrillBotCacheBuilder));
        services.Remove(cacheBuilder);
        services.AddSingleton<GrillBotCacheBuilder>(TestServices.CacheBuilder.Value);
        
        return services.BuildServiceProvider();
    }
}
