using GrillBot.App.Modules.Reminder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Modules.Reminder
{
    [TestClass]
    public class RemindListMetdataTests
    {
        [TestMethod]
        public void TryLoadFrom_FalseCombinations()
        {
            var combinations = new[]
            {
                new Dictionary<string, string>(),
                new Dictionary<string, string>(){ { "Page", "1" } },
                new Dictionary<string, string>(){ { "OfUser", "50" } }
            };

            var metadata = new RemindListMetadata();
            Assert.AreEqual("Reminder", metadata.EmbedKind);

            foreach (var combination in combinations)
            {
                Assert.IsFalse(metadata.TryLoadFrom(combination));

                Assert.AreEqual(default, metadata.Page);
                Assert.AreEqual(default, metadata.OfUser);
            }
        }

        [TestMethod]
        public void TryLoadFrom_TrueCombinations()
        {
            var data = new Dictionary<string, string>()
            {
                { "Page", "1" },
                { "OfUser", "50" }
            };

            var metadata = new RemindListMetadata();
            Assert.AreEqual("Reminder", metadata.EmbedKind);
            Assert.IsTrue(metadata.TryLoadFrom(data));
            Assert.AreEqual(1, metadata.Page);
            Assert.AreEqual((ulong)50, metadata.OfUser);
        }
    }
}
