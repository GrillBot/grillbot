using GrillBot.App.Modules.Points;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Points
{
    [TestClass]
    public class PointsBoardMetadataTests
    {
        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "Page", "1" } },
                new Dictionary<string, string>(){ { "GuildId", "50" } }
            };

            var metadata = new PointsBoardMetadata();
            Assert.AreEqual("Points", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));

                Assert.AreEqual(default, metadata.Page);
                Assert.AreEqual(default, metadata.GuildId);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var data = new Dictionary<string, string>()
            {
                { "Page", "1" },
                { "GuildId", "50" }
            };

            var metadata = new PointsBoardMetadata();
            Assert.AreEqual("Points", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.Page);
            Assert.AreEqual((ulong)50, metadata.GuildId);
        }
    }
}
