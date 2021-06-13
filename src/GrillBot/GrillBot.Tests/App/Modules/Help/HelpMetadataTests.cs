using GrillBot.App.Modules.Help;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Namotion.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Modules.Help
{
    [TestClass]
    public class HelpMetadataTests
    {
        [TestMethod]
        public void SaveInto()
        {
            var metadata = new HelpMetadata()
            {
                Page = 1,
                PagesCount = 50
            };

            var result = new Dictionary<string, string>();
            metadata.SaveInto(result);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(new[] { "Page", "PagesCount" }.SequenceEqual(result.Keys));
            Assert.IsTrue(new[] { "1", "50" }.SequenceEqual(result.Values));
        }

        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "Page", "1" } },
                new Dictionary<string, string>(){ { "PagesCount", "50" } }
            };

            var metadata = new HelpMetadata();
            Assert.AreEqual("Help", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));

                Assert.AreEqual(0, metadata.Page);
                Assert.AreEqual(0, metadata.PagesCount);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var data = new Dictionary<string, string>()
            {
                { "Page", "1" },
                { "PagesCount", "50" }
            };

            var metadata = new HelpMetadata();
            Assert.AreEqual("Help", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.Page);
            Assert.AreEqual(50, metadata.PagesCount);
        }
    }
}
