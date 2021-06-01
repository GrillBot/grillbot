using Discord;
using GrillBot.App.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class ChannelExtensions
    {
        [TestMethod]
        public void GetMention()
        {
            var channel = new Mock<IChannel>();

            channel.Setup(o => o.Id).Returns(615832777669214210);

            Assert.AreEqual("<#615832777669214210>", channel.Object.GetMention());
        }
    }
}
