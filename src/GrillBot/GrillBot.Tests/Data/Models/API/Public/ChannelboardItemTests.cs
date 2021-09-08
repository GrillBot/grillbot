using Discord;
using GrillBot.Data.Models.API.Public;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace GrillBot.Tests.Data.Models.API.Public
{
    [TestClass]
    public class ChannelboardItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new ChannelboardItem());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var channel = new Mock<IChannel>();
            channel.Setup(o => o.Name).Returns("Channel");

            var item = new ChannelboardItem(channel.Object, 50, DateTime.MaxValue);

            Assert.AreEqual(50, item.Count);
            Assert.AreEqual(DateTime.MaxValue, item.LastMessageAt);
            Assert.AreEqual("Channel", item.ChannelName);
        }
    }
}
