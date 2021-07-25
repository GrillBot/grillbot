using Discord;
using GrillBot.Data.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Helpers
{
    [TestClass]
    public class DiscordHelperTests
    {
        [TestMethod]
        public void GetAllIntents()
        {
            const GatewayIntents expected = GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessages | GatewayIntents.DirectMessageTyping | GatewayIntents.GuildBans | GatewayIntents.GuildEmojis | GatewayIntents.GuildIntegrations
                | GatewayIntents.GuildInvites | GatewayIntents.GuildMembers | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping | GatewayIntents.GuildPresences | GatewayIntents.Guilds
                | GatewayIntents.GuildVoiceStates | GatewayIntents.GuildWebhooks;

            var result = DiscordHelper.GetAllIntents();

            Assert.AreEqual(expected, result);
        }
    }
}
