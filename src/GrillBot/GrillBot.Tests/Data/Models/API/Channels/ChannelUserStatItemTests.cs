using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class ChannelUserStatItemTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new ChannelUserStatItem());
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            TestHelpers.CheckNonDefaultPropertyValues(new ChannelUserStatItem()
            {
                Count = 50,
                FirstMessageAt = DateTime.MaxValue,
                LastMessageAt = DateTime.MaxValue,
                Nickname = "Nickname",
                Position = 50,
                UserId = "User",
                Username = "Username"
            });
        }
    }
}
