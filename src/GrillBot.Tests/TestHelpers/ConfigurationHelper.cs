using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class ConfigurationHelper
{
    public static IConfiguration CreateConfiguration(Dictionary<string, string>? externalConfiguration = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Discord:Logging:GuildId", Consts.GuildId.ToString() },
                { "Discord:Logging:ChannelId", Consts.ChannelId.ToString() },
                { "Discord:Logging:Enabled", "true" },
                { "Auth:OAuth2:ClientRedirectUrl", "https://client" },
                { "Auth:OAuth2:AdminRedirectUrl", "https://admin" },
                { "Auth:OAuth2:ClientId", "856879314997346344" },
                { "Auth:OAuth2:ClientSecret", "856879314997346344856879314997346344" },
                { "Discord:Commands:Prefix", "$" },
                { "Discord:Emotes:Sadge", ":sadge:" },
                { "Discord:Emotes:Hypers", ":hypers:" },
                { "Discord:Emotes:Mocking", "<a:mocking:853755944429289482>" },
                { "Reminder:MinimalTimeMinutes", "15" },
                { "Services:Rubbergod:Id", "123456789" },
                { "Services:Rubbergod:AuthorizedChannelId", "987654321" },
                { "Services:Rubbergod:HelpParserClass", "RubbergodHelpParser" },
                { "WebAdmin:UserDetailLink", "http://grillbot/{0}" },
                { "Discord:Emotes:Online", "<:Online:856875667379585034>" },
                { "Discord:Emotes:Offline", "<:Offline:856875666842583040>" },
                { "Discord:Emotes:DoNotDisturb", "<:DoNotDisturb:856879762282774538>" },
                { "Discord:Emotes:Idle", "<:Idle:856879314997346344>" },
                { "Discord:MessageCache:Period", "00:00:00" },
                { "AuditLog:CleaningCron", "00:00:00" },
                { "Reminder:CronJob", "00:00:00" },
                { "Birthday:Cron", "0 0 8 * * ?" },
                { "Unverify:CheckPeriodTime", "00:00:00" },
                { "OnlineUsersCheckPeriodTime", "00:00:00" },
                { "SuggestionsCleaningInterval", "00:00:00" },
                { "Services:KachnaOnline:InfoChannel", "InfoChannel" }
            });

        if (externalConfiguration != null)
            configuration.AddInMemoryCollection(externalConfiguration!);

        return configuration.Build();
    }
}
