using GrillBot.App.Services.Images;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Services.Graphics;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddSingleton<DirectApi.IDirectApiService, DirectApi.DirectApiService>();

        services
            .AddScoped<IGraphicsClient, GraphicsClient>();

        services
            .AddSingleton<FileStorageFactory>();

        services
            .AddScoped<WithoutAccidentRenderer>();

        services
            .AddSingleton<Suggestion.SuggestionSessionService>()
            .AddSingleton<Suggestion.EmoteSuggestionService>();

        services
            .AddSingleton<Unverify.UnverifyChecker>()
            .AddSingleton<Unverify.UnverifyLogger>()
            .AddSingleton<Unverify.UnverifyMessageGenerator>()
            .AddSingleton<Unverify.UnverifyProfileGenerator>()
            .AddSingleton<Unverify.UnverifyService>()
            .AddScoped<Unverify.UnverifyHelper>();

        services
            .AddSingleton<SearchingService>();

        return services;
    }
}
