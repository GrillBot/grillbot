using Discord;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Database.Migrations;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.Data.Extensions.Discord
{
    [TestClass]
    public class ChannelExtensionsTests
    {
        [TestMethod]
        public void IsEqual_InvalidType()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = new Mock<IVoiceChannel>();

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_InvalidId()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.Id).Returns(123456);

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_InvalidName()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.Name).Returns("Jmeno");

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_InvalidPosition()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.Position).Returns(42);

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Text_InvalidCategoryId()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.CategoryId).Returns(60);

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Text_InvalidNsfw()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.IsNsfw).Returns(true);

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Text_InvalidSlowMode()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.SlowModeInterval).Returns(100);

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Text_InvalidTopic()
        {
            var validChannel = CreateValidTextChannel();
            var anotherChannel = CreateValidTextChannel();
            anotherChannel.Setup(o => o.Topic).Returns("Tema");

            var result = validChannel.Object.IsEqual(anotherChannel.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Voice_InvalidBirtate()
        {
            var valid = CreateValidVoiceChannel();
            var another = CreateValidVoiceChannel();
            another.Setup(o => o.Bitrate).Returns(64);

            var result = valid.Object.IsEqual(another.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Voice_InvalidCategoryId()
        {
            var valid = CreateValidVoiceChannel();
            var another = CreateValidVoiceChannel();
            another.Setup(o => o.CategoryId).Returns(121313);

            var result = valid.Object.IsEqual(another.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Voice_InvalidUserLimit()
        {
            var valid = CreateValidVoiceChannel();
            var another = CreateValidVoiceChannel();
            another.Setup(o => o.UserLimit).Returns(64);

            var result = valid.Object.IsEqual(another.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsEqual_Text_Ok()
        {
            var channel = CreateValidTextChannel();

            var result = channel.Object.IsEqual(channel.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEqual_Voice_Ok()
        {
            var channel = CreateValidVoiceChannel();

            var result = channel.Object.IsEqual(channel.Object);
            Assert.IsTrue(result);
        }

        private static Mock<ITextChannel> CreateValidTextChannel()
        {
            var channel = new Mock<ITextChannel>();
            channel.Setup(o => o.Id).Returns(12345);
            channel.Setup(o => o.Name).Returns("Name");
            channel.Setup(o => o.Position).Returns(40);
            channel.Setup(o => o.CategoryId).Returns(50);
            channel.Setup(o => o.IsNsfw).Returns(false);
            channel.Setup(o => o.SlowModeInterval).Returns(0);
            channel.Setup(o => o.Topic).Returns("Topic");

            return channel;
        }

        private static Mock<IVoiceChannel> CreateValidVoiceChannel()
        {
            var channel = new Mock<IVoiceChannel>();
            channel.Setup(o => o.Id).Returns(12345);
            channel.Setup(o => o.Name).Returns("Name");
            channel.Setup(o => o.Position).Returns(40);
            channel.Setup(o => o.CategoryId).Returns(50);
            channel.Setup(o => o.Bitrate).Returns(151);
            channel.Setup(o => o.UserLimit).Returns(10);

            return channel;
        }
    }
}
