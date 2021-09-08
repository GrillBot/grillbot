using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class UpdateChannelParamsTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new UpdateChannelParams());
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            TestHelpers.CheckNonDefaultPropertyValues(new UpdateChannelParams() { Flags = 50 });
        }
    }
}
