using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        services
            .AddScoped<DownloadHelper>()
            .AddScoped<ChannelHelper>()
            .AddSingleton<BlobManagerFactoryHelper>();

        return services;
    }
}
