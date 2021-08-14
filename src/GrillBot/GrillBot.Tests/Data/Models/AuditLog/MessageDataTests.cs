using Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class MessageDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new MessageData().Author);
        }

        [TestMethod]
        public void Constructor_FromMessage()
        {
            var createdAt = new DateTime(2021, 7, 9, 0, 12, 25);

            var author = DiscordHelpers.CreateUserMock(12345, "Username");

            var message = new Mock<IUserMessage>();
            message.Setup(o => o.Author).Returns(author.Object);
            message.Setup(o => o.CreatedAt).Returns(new DateTimeOffset(createdAt));
            message.Setup(o => o.Content).Returns("ABCD");

            var data = new MessageData(message.Object);

            Assert.AreEqual("ABCD", data.Content);
            Assert.AreEqual(createdAt, data.CreatedAt);
        }

        [TestMethod]
        public void Constructor_NullMessage()
        {
            var author = DiscordHelpers.CreateUserMock(12345, "Username");

            var data = new MessageData(author.Object, DateTime.MinValue, null);
            Assert.IsNull(data.Content);
        }
    }
}
