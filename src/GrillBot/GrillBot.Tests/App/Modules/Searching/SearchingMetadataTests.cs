using GrillBot.App.Modules.Searching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Searching
{
    [TestClass]
    public class SearchingMetadataTests
    {
        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "Page", "1" } },
                new Dictionary<string, string>(){ { "ChannelId", "50" } },
                new() { { "GuildId", "100" } }
            };

            var metadata = new SearchingMetadata();
            Assert.AreEqual("Search", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));

                Assert.AreEqual(default, metadata.Page);
                Assert.AreEqual(default, metadata.GuildId);
                Assert.AreEqual(default, metadata.ChannelId);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var data = new Dictionary<string, string>()
            {
                { "Page", "1" },
                { "ChannelId", "50" },
                { "GuildId", "100" }
            };

            var metadata = new SearchingMetadata();
            Assert.AreEqual("Search", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.Page);
            Assert.AreEqual((ulong)50, metadata.ChannelId);
            Assert.AreEqual((ulong)100, metadata.GuildId);
        }

        [TestMethod]
        public void Reset()
        {
            new SearchingMetadata().Reset();
            Assert.IsTrue(true);
        }
    }
}
