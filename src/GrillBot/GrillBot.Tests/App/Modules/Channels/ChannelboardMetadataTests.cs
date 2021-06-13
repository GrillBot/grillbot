using GrillBot.App.Modules.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Channels
{
    [TestClass]
    public class ChannelboardMetadataTests
    {
        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "PageNumber", "1" } },
                new Dictionary<string, string>(){ { "GuildId", "50" } }
            };

            var metadata = new ChannelboardMetadata();
            Assert.AreEqual("Channelboard", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));

                Assert.AreEqual(default, metadata.PageNumber);
                Assert.AreEqual(default, metadata.GuildId);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var data = new Dictionary<string, string>()
            {
                { "PageNumber", "1" },
                { "GuildId", "50" }
            };

            var metadata = new ChannelboardMetadata();
            Assert.AreEqual("Channelboard", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.PageNumber);
            Assert.AreEqual((ulong)50, metadata.GuildId);
        }
    }
}
