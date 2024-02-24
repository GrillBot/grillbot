using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddHelpers(this IServiceCollection services)
    {
        services
            .AddScoped<DownloadHelper>()
            .AddScoped<ChannelHelper>()
            .AddScoped<UnverifyHelper>()
            .AddScoped<EmoteSuggestionHelper>()
            .AddSingleton<BlobManagerFactoryHelper>();

        return services;
    }
}
