using GrillBot.Data.Models.MessageCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.MessageCache
{
    [TestClass]
    public class MessageMetadataTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new MessageMetadata(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void FilledValues()
        {
            var metadata = new MessageMetadata()
            {
                State = GrillBot.Data.Enums.CachedMessageState.NeedsUpdate
            };

            TestHelpers.CheckDefaultPropertyValues(metadata, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
