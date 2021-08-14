using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class GuildChannelTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildChannel(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }
    }
}
