using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.TestHelpers;

[ExcludeFromCodeCoverage]
public static class ConfigurationHelper
{
    public static IConfiguration CreateConfiguration(Dictionary<string, string> externalConfiguration = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>()
            {
                { "Discord:Logging:GuildId", "12345" },
                { "Discord:Logging:ChannelId", "12345" },
                { "Discord:Logging:Enabled", "true" },
                { "OAuth2:ClientRedirectUrl", "https://client" },
                { "OAuth2:AdminRedirectUrl", "https://admin" },
                { "OAuth2:ClientId", "client" },
                { "OAuth2:ClientSecret", "secret" },
                { "AutoReply:DisabledChannels:0", "12345" },
                { "Discord:Commands:Prefix", "$" },
                { "Discord:Emotes:Sadge", ":sadge:" },
                { "Discord:Emotes:Hypers", ":hypers:" },
                { "Discord:Emotes:Mocking", "<a:mocking:853755944429289482>" },
                { "Reminder:MinimalTimeMinutes", "15" }
            });

        if (externalConfiguration != null)
            configuration.AddInMemoryCollection(externalConfiguration);

        return configuration.Build();
    }
}
