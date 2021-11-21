using Discord;
using Discord.WebSocket;
using GrillBot.App.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.App.Services
{
    [TestClass]
    public class EmoteChainServiceTests
    {
        private static EmoteChainService CreateService()
        {
            var client = new DiscordSocketClient();
            var configuration = TestHelper.ConfigHelpers.CreateConfiguration();

            return new EmoteChainService(configuration, client);
        }

        [TestMethod]
        public void Cleanup()
        {
            var service = CreateService();
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(12345);
            var channel = new Mock<IGuildChannel>();
            channel.Setup(o => o.Id).Returns(12345);
            channel.Setup(o => o.Guild).Returns(guild.Object);

            service.Cleanup(channel.Object);
            Assert.IsTrue(true);
        }
    }
}
