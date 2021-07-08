using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class AuditChannelInfoTests
    {
        [TestMethod]
        public void Constructor_Text()
        {
            var channel = new Mock<ITextChannel>();
            channel.Setup(o => o.Id).Returns(12345);
            channel.Setup(o => o.Name).Returns("Channel");
            channel.Setup(o => o.IsNsfw).Returns(true);
            channel.Setup(o => o.SlowModeInterval).Returns(0);
            channel.Setup(o => o.Topic).Returns("Topic");

            var auditChannel = new AuditChannelInfo(channel.Object);

            Assert.AreEqual((ulong)12345, auditChannel.Id);
            Assert.AreEqual("Channel", auditChannel.Name);
            Assert.AreEqual(true, auditChannel.IsNsfw);
            Assert.AreEqual(0, auditChannel.SlowMode);
            Assert.AreEqual("Topic", auditChannel.Topic);
            Assert.AreEqual(ChannelType.Text, auditChannel.Type);
            Assert.AreEqual(null, auditChannel.Bitrate);
        }

        [TestMethod]
        public void EmptyConstructor()
        {
            var auditChannel = new AuditChannelInfo();
            Assert.AreEqual((ulong)0, auditChannel.Id);
        }

        [TestMethod]
        public void Constructor_Voice()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);
            channel.Setup(o => o.Name).Returns("Channel");
            channel.Setup(o => o.Bitrate).Returns(64000);

            var auditChannel = new AuditChannelInfo(channel.Object);

            Assert.AreEqual((ulong)12345, auditChannel.Id);
            Assert.AreEqual("Channel", auditChannel.Name);
            Assert.AreEqual(null, auditChannel.IsNsfw);
            Assert.AreEqual(null, auditChannel.SlowMode);
            Assert.AreEqual(null, auditChannel.Topic);
            Assert.AreEqual(ChannelType.Voice, auditChannel.Type);
            Assert.AreEqual(64000, auditChannel.Bitrate);
        }

        [TestMethod]
        public void CompareTo_False()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            Assert.AreEqual(1, auditChannel.CompareTo(new AuditChannelInfo()));
            Assert.AreEqual(1, auditChannel.CompareTo(new object()));
        }

        [TestMethod]
        public void CompareTo_True()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            Assert.AreEqual(0, auditChannel.CompareTo(new AuditChannelInfo(channel.Object)));
        }

        [TestMethod]
        public void Equals_True()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            Assert.IsTrue(auditChannel.Equals(new AuditChannelInfo(channel.Object)));
        }

        [TestMethod]
        public void Equals_False()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            Assert.IsFalse(auditChannel.Equals(new AuditChannelInfo()));
        }

        [TestMethod]
        public void Operators_True()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            var anotherChannel = new AuditChannelInfo(channel.Object);
            var emptyChannel = new AuditChannelInfo();

            Assert.IsTrue(auditChannel == anotherChannel);
            Assert.IsTrue(auditChannel != emptyChannel);
            Assert.IsTrue(auditChannel < emptyChannel);
            Assert.IsTrue(auditChannel > emptyChannel);
            Assert.IsTrue(auditChannel <= emptyChannel);
            Assert.IsTrue(auditChannel >= emptyChannel);
        }

        [TestMethod]
        public void Operators_False()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            var anotherChannel = new AuditChannelInfo(channel.Object);
            var emptyChannel = new AuditChannelInfo();

            Assert.IsFalse(auditChannel == emptyChannel);
            Assert.IsFalse(auditChannel != anotherChannel);
            Assert.IsFalse(auditChannel < anotherChannel);
            Assert.IsFalse(auditChannel > anotherChannel);
            Assert.IsFalse(auditChannel <= anotherChannel);
            Assert.IsFalse(auditChannel >= anotherChannel);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);

            var auditChannel = new AuditChannelInfo(channel.Object);
            Assert.IsTrue(auditChannel.GetHashCode() != 0);
        }
    }
}
