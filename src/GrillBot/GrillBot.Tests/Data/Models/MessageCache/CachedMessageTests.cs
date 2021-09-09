using Discord;
using GrillBot.Data.Models.MessageCache;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.MessageCache
{
    [TestClass]
    public class CachedMessageTests
    {
        [TestMethod]
        public void Constructor()
        {
            var msg = new Mock<IMessage>();
            var message = new CachedMessage(msg.Object);

            TestHelpers.CheckNonDefaultPropertyValues(message);
        }
    }
}
