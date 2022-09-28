using GrillBot.App;
using GrillBot.Data.Models.AuditLog;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        
        var dbBuilder = services.FirstOrDefault(o => o.ServiceType == typeof(GrillBotDatabaseBuilder));
        services.Remove(dbBuilder);
        services.AddSingleton<GrillBotDatabaseBuilder>(TestServices.DatabaseBuilder.Value);
        
        return services.BuildServiceProvider();
    }
}
