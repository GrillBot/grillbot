using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace GrillBot.Tests.TestHelper
{
    public static class ConfigHelpers
    {
        public static IConfiguration CreateConfiguration(int reminderMinimalTime = 0, Dictionary<string, string> inMemoryCollection = null)
        {
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>()
                {
                    { "Discord:Emotes:Sadge", "<sadge>" },
                    { "Discord:Emotes:Hypers", "<hypers>" },
                    { "Reminder:MinimalTimeMinutes", reminderMinimalTime.ToString() },
                    { "Discord:Commands:Prefix", "$" }
                });

            if (inMemoryCollection != null)
                builder.AddInMemoryCollection(inMemoryCollection);

            return builder.Build();
        }
    }
}
