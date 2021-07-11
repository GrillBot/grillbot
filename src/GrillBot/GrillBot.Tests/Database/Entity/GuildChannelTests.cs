using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class GuildChannelTests
    {
        [TestMethod]
        public void HasFlags_True()
        {
            var channel = new GuildChannel()
            {
                Flags = (int)GuildChannelFlags.IgnoreCache
            };

            Assert.IsTrue(channel.HasFlags(GuildChannelFlags.IgnoreCache));
        }

        [TestMethod]
        public void HasFlags_False()
        {
            var channel = new GuildChannel();
            Assert.IsFalse(channel.HasFlags(GuildChannelFlags.IgnoreCache));
        }
    }
}
