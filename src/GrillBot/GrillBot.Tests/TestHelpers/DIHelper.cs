using GrillBot.App;
using GrillBot.Data.Models.AuditLog;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class DIHelper
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
            .AddSingleton(EnvironmentHelper.CreateEnv("Testing"));

        startup.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
