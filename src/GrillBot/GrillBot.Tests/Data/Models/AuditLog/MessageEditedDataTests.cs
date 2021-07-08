using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class MessageEditedDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new MessageEditedData().Diff);
        }

        [TestMethod]
        public void Constructor_WithMessage()
        {
            var channel = new Mock<IDMChannel>();
            channel.Setup(o => o.Id).Returns(123);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Content).Returns("ABCD");
            message.Setup(o => o.Id).Returns(12345);
            message.Setup(o => o.Channel).Returns(channel.Object);

            var data = new MessageEditedData(message.Object, message.Object);

            Assert.IsTrue(data.Diff.IsEmpty);
            Assert.IsNotNull(data.JumpUrl);
        }
    }
}
