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
            TestHelpers.CheckDefaultPropertyValues(new MessageMetadata());
        }

        [TestMethod]
        public void FilledValues()
        {
            var metadata = new MessageMetadata()
            {
                State = GrillBot.Data.Enums.CachedMessageState.NeedsUpdate
            };

            TestHelpers.CheckNonDefaultPropertyValues(metadata);
        }
    }
}
