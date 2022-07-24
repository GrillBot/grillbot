using GrillBot.Common.FileStorage;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddSingleton<AuditLog.AuditLogService>()
            .AddScoped<AuditLog.AuditLogApiService>()
            .AddSingleton<AuditLog.AuditLogWriter>();

        services
            .AddSingleton<AutoReply.AutoReplyService>()
            .AddScoped<AutoReply.AutoReplyApiService>();

        services
            .AddSingleton<Birthday.BirthdayService>();

        services
            .AddScoped<CommandsHelp.CommandsHelpService>()
            .AddScoped<CommandsHelp.ExternalCommandsHelpService>();

        services
            .AddSingleton<DirectApi.DirectApiService>();

        services
            .AddSingleton<Discord.DiscordSyncService>();

        services
            .AddSingleton<Emotes.EmoteService>()
            .AddSingleton<Emotes.EmoteChainService>()
            .AddSingleton<Emotes.EmotesCacheService>()
            .AddScoped<Emotes.EmotesApiService>()
            .AddSingleton<Emotes.EmotesCommandService>();

        services
            .AddSingleton<FileStorageFactory>();

        services
            .AddSingleton<Guild.GuildEventsService>()
            .AddScoped<Guild.GuildApiService>();

        services
            .AddSingleton<Channels.ChannelService>()
            .AddScoped<Channels.ChannelApiService>();

        services
            .AddSingleton<Logging.LoggingService>();

        services
            .AddSingleton<Permissions.PermissionsCleaner>()
            .AddSingleton<Permissions.PermissionsService>();

        services
            .AddSingleton<Reminder.RemindService>()
            .AddScoped<Reminder.RemindApiService>();

        services
            .AddSingleton<Suggestion.SuggestionSessionService>()
            .AddSingleton<Suggestion.EmoteSuggestionService>()
            .AddSingleton<Suggestion.FeatureSuggestionService>()
            .AddScoped<Suggestion.EmoteSuggestionApiService>()
            .AddSingleton<Suggestion.EmoteSuggestionsEventManager>();

        services
            .AddSingleton<Unverify.UnverifyChecker>()
            .AddSingleton<Unverify.UnverifyLogger>()
            .AddSingleton<Unverify.UnverifyProfileGenerator>()
            .AddSingleton<Unverify.UnverifyService>()
            .AddSingleton<Unverify.SelfunverifyService>()
            .AddScoped<Unverify.UnverifyApiService>();

        services
            .AddSingleton<User.UserService>()
            .AddSingleton<User.PointsService>()
            .AddScoped<User.RubbergodKarmaService>()
            .AddScoped<User.UsersApiService>()
            .AddScoped<User.UserHearthbeatService>();

        services
            .AddSingleton<BoosterService>()
            .AddSingleton<InviteService>()
            .AddSingleton<MockingService>()
            .AddScoped<OAuth2Service>()
            .AddSingleton<RandomizationService>()
            .AddSingleton<SearchingService>();

        return services;
    }
}
