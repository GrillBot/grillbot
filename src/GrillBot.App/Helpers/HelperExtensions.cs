using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        services
            .AddScoped<PointsHelper>()
            .AddScoped<EmoteHelper>()
            .AddScoped<DownloadHelper>()
            .AddScoped<ChannelHelper>()
            .AddScoped<UnverifyHelper>();

        return services;
    }
}
