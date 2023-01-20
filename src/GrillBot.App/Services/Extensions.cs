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

        services
            .AddSingleton<Suggestion.SuggestionSessionService>()
            .AddSingleton<Suggestion.EmoteSuggestionService>();

        services
            .AddSingleton<Unverify.UnverifyChecker>()
            .AddSingleton<Unverify.UnverifyMessageGenerator>()
            .AddSingleton<Unverify.UnverifyProfileGenerator>()
            .AddSingleton<Unverify.UnverifyService>()
            .AddScoped<Unverify.UnverifyHelper>();

        return services;
    }
}
