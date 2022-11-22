using GrillBot.App.Services.Images;
using GrillBot.Common.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddSingleton<AuditLog.AuditLogService>()
            .AddSingleton<AuditLog.AuditLogWriter>()
            .AddSingleton<AuditLog.AuditClearingHelper>();

        services
            .AddSingleton<AutoReplyService>();

        services
            .AddSingleton<Birthday.BirthdayService>();

        services
            .AddScoped<CommandsHelp.ExternalCommandsHelpService>();

        services
            .AddSingleton<DirectApi.IDirectApiService, DirectApi.DirectApiService>();

        services
            .AddSingleton<Discord.DiscordSyncService>();

        services
            .AddSingleton<Emotes.EmoteService>()
            .AddSingleton<Emotes.EmoteChainService>()
            .AddSingleton<Emotes.EmotesCommandService>();

        services
            .AddSingleton<FileStorageFactory>();

        services
            .AddScoped<WithoutAccidentRenderer>();

        services
            .AddSingleton<Channels.ChannelService>();

        services
            .AddSingleton<Suggestion.SuggestionSessionService>()
            .AddSingleton<Suggestion.EmoteSuggestionService>()
            .AddSingleton<Suggestion.EmoteSuggestionsEventManager>();

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
            .AddSingleton<BoosterService>()
            .AddSingleton<InviteService>()
            .AddSingleton<MockingService>()
            .AddSingleton<RandomizationService>()
            .AddSingleton<SearchingService>();

        return services;
    }
}
