using GrillBot.App;
using GrillBot.Data.Models.AuditLog;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GrillBot.Cache.Services;
using GrillBot.Database.Services;

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

        ReplaceService<GrillBotDatabaseBuilder>(services, TestServices.DatabaseBuilder.Value);
        ReplaceService<GrillBotCacheBuilder>(services, TestServices.CacheBuilder.Value);
        ReplaceService(services, new GraphicsClientBuilder().SetAll().Build());
        return services.BuildServiceProvider();
    }

    private static void ReplaceService<TType>(IServiceCollection services, TType replacement)
    {
        var service = services.First(o => o.ServiceType == typeof(TType));
        services.Remove(service);
        services.Add(new ServiceDescriptor(typeof(TType), replacement!));
    }
}
