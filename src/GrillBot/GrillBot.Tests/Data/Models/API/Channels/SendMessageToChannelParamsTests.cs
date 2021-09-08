using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class SendMessageToChannelParamsTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new SendMessageToChannelParams());
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            TestHelpers.CheckNonDefaultPropertyValues(new SendMessageToChannelParams()
            {
                Content = "Content",
                Reference = "Reference"
            });
        }
    }
}
