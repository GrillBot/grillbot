using Discord;
using Discord.WebSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Services.MessageCache
{
    [TestClass]
    public class MessageCacheTests
    {
        [TestMethod]
        public void ComplexTest()
        {
            var client = new DiscordSocketClient();
            var cache = new GrillBot.App.Services.MessageCache.MessageCache(client);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(12345);
            var secondMessage = new Mock<IMessage>();
            secondMessage.Setup(o => o.Id).Returns(123456);

            var messages = new List<IMessage>() { message.Object, secondMessage.Object }.AsReadOnly();
            var messagesContainer = new List<IReadOnlyCollection<IMessage>>() { messages }.ToAsyncEnumerable();

            var channelMock = new Mock<ISocketMessageChannel>();
            channelMock.Setup(o => o.GetMessagesAsync(It.IsAny<ulong>(), It.IsAny<Direction>(), It.IsAny<int>(), It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>())).Returns(messagesContainer);
            var channel = channelMock.Object;

            cache.AppendAroundAsync(channel, 12345).Wait();
            var afterGet = cache.GetMessageAsync(channel, 123456).Result;

            Assert.AreEqual((ulong)123456, afterGet.Id);

            var notFound = cache.GetMessage(1);
            Assert.IsNull(notFound);

            Assert.IsTrue(cache.TryRemove(123456, out var _));
            notFound = cache.GetMessage(123456);
            Assert.IsNull(notFound);

            Assert.IsFalse(cache.TryRemove(456, out var _));
            Assert.IsNotNull(cache.GetMessage(123456, true));
        }

        [TestMethod]
        public void MarkUpdated()
        {
            var client = new DiscordSocketClient();
            var cache = new GrillBot.App.Services.MessageCache.MessageCache(client);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(12345);
            var secondMessage = new Mock<IMessage>();
            secondMessage.Setup(o => o.Id).Returns(123456);

            var messages = new List<IMessage>() { message.Object, secondMessage.Object }.AsReadOnly();
            var messagesContainer = new List<IReadOnlyCollection<IMessage>>() { messages }.ToAsyncEnumerable();

            var channelMock = new Mock<ISocketMessageChannel>();
            channelMock.Setup(o => o.GetMessagesAsync(It.IsAny<ulong>(), It.IsAny<Direction>(), It.IsAny<int>(), It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>())).Returns(messagesContainer);
            var channel = channelMock.Object;

            cache.AppendAroundAsync(channel, 12345).Wait();
            cache.MarkUpdated(12345);
            cache.MarkUpdated(123);
            Assert.IsNotNull(cache.GetMessage(12345));
        }

        [TestMethod]
        public void AppendAround_DMs()
        {
            var client = new DiscordSocketClient();
            var cache = new GrillBot.App.Services.MessageCache.MessageCache(client);

            var channelMock = new Mock<IDMChannel>();
            var channel = channelMock.Object;

            cache.AppendAroundAsync(channel, 12345).Wait();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ClearChannel()
        {
            var client = new DiscordSocketClient();
            var cache = new GrillBot.App.Services.MessageCache.MessageCache(client);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(12345);
            message.Setup(o => o.Channel).Returns(() =>
            {
                var mock = new Mock<IMessageChannel>();
                mock.Setup(o => o.Id).Returns(1);
                return mock.Object;
            });
            var secondMessage = new Mock<IMessage>();
            secondMessage.Setup(o => o.Id).Returns(123456);
            secondMessage.Setup(o => o.Channel).Returns(() =>
            {
                var mock = new Mock<IMessageChannel>();
                mock.Setup(o => o.Id).Returns(1);
                return mock.Object;
            });

            var messages = new List<IMessage>() { message.Object, secondMessage.Object }.AsReadOnly();
            var messagesContainer = new List<IReadOnlyCollection<IMessage>>() { messages }.ToAsyncEnumerable();

            var channelMock = new Mock<ISocketMessageChannel>();
            channelMock.Setup(o => o.GetMessagesAsync(It.IsAny<ulong>(), It.IsAny<Direction>(), It.IsAny<int>(), It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>())).Returns(messagesContainer);
            var channel = channelMock.Object;

            cache.AppendAroundAsync(channel, 12345).Wait();
            cache.TryRemove(12345, out var _);
            Assert.AreEqual(1, cache.ClearChannel(1));
        }

        [TestMethod]
        public void GetMessageWithDownload()
        {
            var client = new DiscordSocketClient();
            var cache = new GrillBot.App.Services.MessageCache.MessageCache(client);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(12345);
            var secondMessage = new Mock<IMessage>();
            secondMessage.Setup(o => o.Id).Returns(123456);

            var messages = new List<IMessage>() { message.Object, secondMessage.Object }.AsReadOnly();
            var messagesContainer = new List<IReadOnlyCollection<IMessage>>() { messages }.ToAsyncEnumerable();

            var channelMock = new Mock<ISocketMessageChannel>();
            channelMock.Setup(o => o.GetMessagesAsync(It.IsAny<ulong>(), It.IsAny<Direction>(), It.IsAny<int>(), It.IsAny<CacheMode>(),
                It.IsAny<RequestOptions>())).Returns(messagesContainer);
            channelMock.Setup(o => o.GetMessageAsync(It.Is<ulong>(x => x == 12345), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(message.Object));
            var channel = channelMock.Object;

            var msg = cache.GetMessageAsync(channel, 12345).Result;
            Assert.IsNotNull(msg);

            msg = cache.GetMessageAsync(channel, 1).Result;
            Assert.IsNull(msg);
        }
    }
}
