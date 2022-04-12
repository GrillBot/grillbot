using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddSingleton<Emotes.EmotesCacheService>()
            .AddScoped<Emotes.EmotesApiService>()
            .AddSingleton<Emotes.EmotesCommandService>();

        return services;
    }
}
