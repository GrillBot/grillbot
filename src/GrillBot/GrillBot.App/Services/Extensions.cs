using GrillBot.App.Services.Images;
using GrillBot.Common.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<CommandsHelp.ExternalCommandsHelpService>();

        services
            .AddSingleton<DirectApi.IDirectApiService, DirectApi.DirectApiService>();

        services
            .AddSingleton<Emotes.EmotesCommandService>();

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
            .AddSingleton<User.Points.PointsService>();
        
        services
            .AddSingleton<User.UserService>()
            .AddScoped<User.UserHearthbeatService>();

        services
            .AddSingleton<MockingService>()
            .AddSingleton<SearchingService>();

        return services;
    }
}
