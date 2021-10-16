using Discord;
using GrillBot.App.Extensions.Discord;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class MessageExtensionsTests
    {
        [TestMethod]
        public void IsCommand_Mention()
        {
            var user = DiscordHelpers.CreateUserMock(370506820197810176, null);

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("<@370506820197810176> hello");

            int argPos = 0;
            var result = msg.Object.IsCommand(ref argPos, user.Object, "$");

            Assert.AreNotEqual(0, argPos);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsCommand_NoLength()
        {
            var user = DiscordHelpers.CreateUserMock(0, null);
            user.Setup(o => o.Mention).Returns("<@1234>");

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("");

            int argPos = 0;
            var result = msg.Object.IsCommand(ref argPos, user.Object, "$");

            Assert.AreEqual(0, argPos);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsCommand_StringPrefix()
        {
            var user = DiscordHelpers.CreateUserMock(0, null);
            user.Setup(o => o.Mention).Returns("<@1234>");

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("$hello");

            int argPos = 0;
            var result = msg.Object.IsCommand(ref argPos, user.Object, "$");

            Assert.AreNotEqual(0, argPos);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsCommand_WithoutArgPos()
        {
            var user = DiscordHelpers.CreateUserMock(0, null);
            user.Setup(o => o.Mention).Returns("<@1234>");

            var msg = new Mock<IUserMessage>();
            msg.Setup(o => o.Content).Returns("$hello");

            Assert.IsTrue(msg.Object.IsCommand(user.Object, "$"));
        }

        [TestMethod]
        public void GetEmotesFromMessages_WithoutSupportedEmotes()
        {
            const string rtzW = "<:rtzW:567039874452946961>";

            var emojiTag = new Mock<ITag>();
            emojiTag.Setup(o => o.Type).Returns(TagType.Emoji);
            emojiTag.Setup(o => o.Value).Returns(Emote.Parse(rtzW));

            var channelTag = new Mock<ITag>();
            channelTag.Setup(o => o.Type).Returns(TagType.ChannelMention);
            channelTag.Setup(o => o.Value).Returns(new Mock<IChannel>().Object);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Tags).Returns(new List<ITag>() { emojiTag.Object, channelTag.Object });

            var emotes = message.Object.GetEmotesFromMessage(null).ToList();

            Assert.AreEqual(1, emotes.Count);
            Assert.AreEqual(rtzW, emotes[0].ToString());
        }

        [TestMethod]
        public void GetEmotesFromMessages_WithEmotes()
        {
            var emojiTag = new Mock<ITag>();
            emojiTag.Setup(o => o.Type).Returns(TagType.Emoji);
            emojiTag.Setup(o => o.Value).Returns(Emote.Parse("<:rtzW:567039874452946961>"));

            var channelTag = new Mock<ITag>();
            channelTag.Setup(o => o.Type).Returns(TagType.ChannelMention);
            channelTag.Setup(o => o.Value).Returns(new Mock<IChannel>().Object);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Tags).Returns(new List<ITag>() { emojiTag.Object, channelTag.Object });

            var emotes = message.Object.GetEmotesFromMessage(new List<GuildEmote>()).ToList();

            Assert.AreEqual(0, emotes.Count);
        }

        [TestMethod]
        public void Download_OK()
        {
            var attachment = new Mock<IAttachment>();
            attachment.Setup(o => o.Url).Returns("http://google.cz/index.html");

            var result = attachment.Object.DownloadAsync().Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Download_ProxyUrl_OK()
        {
            var attachment = new Mock<IAttachment>();
            attachment.Setup(o => o.Url).Returns("http://google.cz/bagr.html");
            attachment.Setup(o => o.ProxyUrl).Returns("http://google.cz/index.html");

            var result = attachment.Object.DownloadAsync().Result;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Download_Failed()
        {
            var attachment = new Mock<IAttachment>();
            attachment.Setup(o => o.Url).Returns("http://google.cz/bagr.html");
            attachment.Setup(o => o.ProxyUrl).Returns("http://google.cz/testddd.html");

            var result = attachment.Object.DownloadAsync().Result;
            Assert.IsNull(result);
        }
    }
}
