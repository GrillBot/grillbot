using GrillBot.App.Modules.User;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.User
{
    [TestClass]
    public class UserAccessListMetadataTests
    {
        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "Page", "1" } },
                new Dictionary<string, string>(){ { "GuildId", "50" } },
                new Dictionary<string, string>(){ { "ForUserId", "30" } }
            };

            var metadata = new UserAccessListMetadata();
            Assert.AreEqual("UserAccessList", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));

                Assert.AreEqual(default, metadata.Page);
                Assert.AreEqual(default, metadata.GuildId);
                Assert.AreEqual(default, metadata.ForUserId);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var data = new Dictionary<string, string>()
            {
                { "Page", "1" },
                { "GuildId", "50" },
                { "ForUserId", "30" }
            };

            var metadata = new UserAccessListMetadata();
            Assert.AreEqual("UserAccessList", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.Page);
            Assert.AreEqual((ulong)30, metadata.ForUserId);
            Assert.AreEqual((ulong)50, metadata.GuildId);
        }

        [TestMethod]
        public void Reset()
        {
            new UserAccessListMetadata().Reset();
            Assert.IsTrue(true);
        }
    }
}
