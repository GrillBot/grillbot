using GrillBot.App.Modules.Implementations.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Implementations.Unverify
{
    [TestClass]
    public class UnverifyListMetadataTests
    {
        [TestMethod]
        public void SaveInto()
        {
            var dict = new Dictionary<string, string>();
            var metadata = new UnverifyListMetadata() { GuildId = 12345, Page = 1 };

            metadata.SaveInto(dict);

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("12345", dict["GuildId"]);
            Assert.AreEqual("1", dict["Page"]);
        }

        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "Page", "1" } },
                new Dictionary<string, string>(){ { "GuildId", "50" } }
            };

            var metadata = new UnverifyListMetadata();
            Assert.AreEqual("UnverifyList", metadata.EmbedKind);

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

            var metadata = new UnverifyListMetadata();
            Assert.AreEqual("UnverifyList", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.Page);
            Assert.AreEqual((ulong)50, metadata.GuildId);
        }
    }
}
