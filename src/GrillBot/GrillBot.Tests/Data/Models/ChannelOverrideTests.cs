using Discord;
using GrillBot.Data.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models
{
    [TestClass]
    public class ChannelOverrideTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new ChannelOverride(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void Constructor()
        {
            var channel = new Mock<IChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var permissions = new OverwritePermissions(1024, 1024);
            var @override = new ChannelOverride(channel.Object, permissions);

            Assert.AreEqual((ulong)12345, @override.ChannelId);
            Assert.AreEqual((ulong)1024, @override.AllowValue);
            Assert.AreEqual((ulong)1024, @override.DenyValue);
        }
    }
}
