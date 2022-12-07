using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App;

public static class ServiceExtensions
{
    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        services
            .AddScoped<PointsHelper>()
            .AddScoped<EmoteHelper>();

        return services;
    }

    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services
            .AddScoped<PointsRecalculationManager>()
            .AddSingleton<AutoReplyManager>()
            .AddSingleton<EmoteChainManager>();
        return services;
    }
}
