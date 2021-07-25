using Discord;
using GrillBot.Data.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class ChannelExtensions
    {
        [TestMethod]
        public void GetMention()
        {
            var channel = new Mock<IChannel>();

            channel.Setup(o => o.Id).Returns(615832777669214210);

            Assert.AreEqual("<#615832777669214210>", channel.Object.GetMention());
        }
    }
}
