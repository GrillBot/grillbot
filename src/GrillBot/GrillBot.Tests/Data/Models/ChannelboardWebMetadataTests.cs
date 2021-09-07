using GrillBot.Data.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models
{
    [TestClass]
    public class ChannelboardWebMetadataTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new ChannelboardWebMetadata(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void FilledValues()
        {
            var metadata = new ChannelboardWebMetadata()
            {
                GuildId = 12345,
                UserId = 12345
            };

            TestHelpers.CheckDefaultPropertyValues(metadata, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
