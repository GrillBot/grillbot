using GrillBot.Data.Models.API.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyLogSetTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogSet());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var item = new UnverifyLogSet(new GrillBot.Data.Models.Unverify.UnverifyLogSet()
            {
                ChannelsToKeep = new() { new() { ChannelId = 1234 } },
                ChannelsToRemove = new() { new() { ChannelId = 13556 } },
                RolesToKeep = new(),
                RolesToRemove = new()
            }, null);

            Assert.AreEqual(1, item.ChannelIdsToKeep.Count);
            Assert.AreEqual(1, item.ChannelIdsToRemove.Count);
        }
    }
}
