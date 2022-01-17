using GrillBot.Data.Modules.Implementations.Emotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Implementations.Emotes
{
    [TestClass]
    public class EmoteListMetadataTests
    {
        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){{ "Page", "1" }, { "Desc", "False" }, { "SortBy", "count" }, { "OfUserId", "1234" }},
                new Dictionary<string, string>(){{ "IsPrivate", "False" }, { "Desc", "False" }, { "SortBy", "count" }, { "OfUserId", "1234" }},
                new Dictionary<string, string>(){ { "Page", "1" }, { "IsPrivate", "False" }, { "SortBy", "count" }, { "OfUserId", "1234" }},
                new Dictionary<string, string>(){{ "Page", "1" }, { "IsPrivate", "False" }, { "Desc", "False" }, { "OfUserId", "1234" }}
            };

            var metadata = new EmoteListMetadata();
            Assert.AreEqual("EmoteList", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));
                Assert.IsFalse(metadata.IsPrivate);
                Assert.AreEqual(0, metadata.Page);
                Assert.IsFalse(metadata.Desc);
                Assert.IsNull(metadata.OfUserId);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(){{ "Page", "1" }, { "IsPrivate", "False" }, { "Desc", "True" }, { "SortBy", "count" }, { "OfUserId", "1234" }},
                new Dictionary<string, string>(){{ "Page", "1" }, { "IsPrivate", "False" }, { "Desc", "True" }, { "SortBy", "count" }},
            };

            var metadata = new EmoteListMetadata();
            Assert.AreEqual("EmoteList", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsTrue(metadata.TryLoadFrom(combination));
                Assert.IsFalse(metadata.IsPrivate);
                Assert.AreEqual(1, metadata.Page);
                Assert.IsTrue(metadata.Desc);
                Assert.IsTrue(metadata.OfUserId == null || metadata.OfUserId == 1234);
            }
        }
    }
}