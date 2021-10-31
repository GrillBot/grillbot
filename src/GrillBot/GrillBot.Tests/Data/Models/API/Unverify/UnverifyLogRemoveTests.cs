using GrillBot.Data.Models.API.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyLogRemoveTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogRemove());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var item = new UnverifyLogRemove(new GrillBot.Data.Models.Unverify.UnverifyLogRemove()
            {
                ReturnedOverwrites = new() { new() { ChannelId = 12345 } },
                ReturnedRoles = new()
            }, null);

            Assert.AreEqual(1, item.ReturnedChannelIds.Count);
        }
    }
}
