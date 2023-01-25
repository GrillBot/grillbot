using GrillBot.App.Managers.EmoteSuggestion;
using GrillBot.Common.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddSingleton<DirectApi.IDirectApiService, DirectApi.DirectApiService>();

        services
            .AddSingleton<FileStorageFactory>();

        return services;
    }
}
