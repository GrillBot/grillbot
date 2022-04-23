using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services
            .AddScoped<AuditLog.AuditLogApiService>();

        services
            .AddScoped<AutoReply.AutoReplyApiService>();

        services
            .AddSingleton<Emotes.EmotesCacheService>()
            .AddScoped<Emotes.EmotesApiService>()
            .AddSingleton<Emotes.EmotesCommandService>();

        services
            .AddScoped<Guild.GuildApiService>();

        services
            .AddScoped<Channels.ChannelApiService>();

        services
            .AddScoped<Reminder.RemindApiService>();

        services
            .AddSingleton<Suggestion.SuggestionService>()
            .AddSingleton<Suggestion.SuggestionSessionService>()
            .AddSingleton<Suggestion.EmoteSuggestionService>()
            .AddSingleton<Suggestion.FeatureSuggestionService>();

        services
            .AddScoped<User.UsersApiService>();

        return services;
    }
}
