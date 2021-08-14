using Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class MessageDeletedDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new MessageDeletedData().Data);
        }

        [TestMethod]
        public void Constructor()
        {
            var createdAt = new DateTime(2021, 7, 9, 0, 12, 25);

            var author = DiscordHelpers.CreateUserMock(12345, "Username");

            var message = new Mock<IUserMessage>();
            message.Setup(o => o.Author).Returns(author.Object);
            message.Setup(o => o.CreatedAt).Returns(new DateTimeOffset(createdAt));
            message.Setup(o => o.Content).Returns("ABCD");

            var data = new MessageDeletedData(message.Object);

            Assert.IsNotNull(data.Data);
        }

        [TestMethod]
        public void Constructor_NoData()
        {
            var data = new MessageDeletedData((IUserMessage)null);
            Assert.IsFalse(data.Cached);
            Assert.IsNull(data.Data);
        }
    }
}
