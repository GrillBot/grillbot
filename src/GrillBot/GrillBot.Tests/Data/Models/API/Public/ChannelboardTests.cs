using GrillBot.Data.Models.API.Public;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.API.Public
{
    [TestClass]
    public class ChannelboardTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new Channelboard());
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            var channelboard = new Channelboard()
            {
                Channels = new List<ChannelboardItem>(),
                Guild = new(),
                SessionId = "Session",
                User = new()
            };

            TestHelpers.CheckNonDefaultPropertyValues(channelboard);
        }
    }
}
