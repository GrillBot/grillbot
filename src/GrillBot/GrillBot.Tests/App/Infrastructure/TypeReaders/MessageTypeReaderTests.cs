using Discord;
using Discord.Commands;
using GrillBot.App.Infrastructure.TypeReaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders
{
    [TestClass]
    public class MessageTypeReaderTests
    {
        [TestMethod]
        public void Read_InvalidUri()
        {
            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(null, "message", null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }

        [TestMethod]
        public void Read_InvalidRegexMatch()
        {
            const string uri = "http://discord.com";

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(null, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }

        [TestMethod]
        public void Read_DMs()
        {
            const string uri = "https://discord.com/channels/@me/604712748793724966/858635240830009366";

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(null, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.Unsuccessful, result.Error);
        }

        [TestMethod]
        public void Read_InvalidGuildId()
        {
            const string uri = "https://discord.com/channels/597759515615615616591178174465/604712748793724966/858635240830009366";

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(null, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }

        [TestMethod]
        public void Read_GuildNotFound()
        {
            const string uri = "https://discord.com/channels/597759591178174465/604712748793724966/858635240830009366";

            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(null as IGuild));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ObjectNotFound, result.Error);
        }

        [TestMethod]
        public void Read_InvalidChannelId()
        {
            const string uri = "https://discord.com/channels/597759591178174465/6047145615616162748793724966/858635240830009366";

            var guild = new Mock<IGuild>();
            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild.Object));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }

        [TestMethod]
        public void Read_ChannelNotFound()
        {
            const string uri = "https://discord.com/channels/597759591178174465/604712748793724966/858635240830009366";

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.GetTextChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(null as ITextChannel));

            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild.Object));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ObjectNotFound, result.Error);
        }

        [TestMethod]
        public void Read_InvalidMessageId()
        {
            const string uri = "https://discord.com/channels/597759591178174465/604712748793724966/858635240830116161616009366";

            var channel = new Mock<ITextChannel>();
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.GetTextChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel.Object));

            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild.Object));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ParseFailed, result.Error);
        }

        [TestMethod]
        public void Read_MessageNotFound()
        {
            const string uri = "https://discord.com/channels/597759591178174465/604712748793724966/858635240830009366";

            var channel = new Mock<ITextChannel>();
            channel.Setup(o => o.GetMessageAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(null as IMessage));

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.GetTextChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel.Object));

            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild.Object));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, uri, null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(CommandError.ObjectNotFound, result.Error);
        }

        [TestMethod]
        public void Read_Success()
        {
            const string uri = "https://discord.com/channels/597759591178174465/604712748793724966/858635240830009366";

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(12345);

            var channel = new Mock<ITextChannel>();
            channel.Setup(o => o.GetMessageAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(message.Object));

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.GetTextChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel.Object));

            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild.Object));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, uri, null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);

            var msg = result.Values.First().Value as IMessage;
            Assert.AreEqual((ulong)12345, msg.Id);
        }

        [TestMethod]
        public void Read_Success_FromId()
        {
            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(12345);

            var channel = new Mock<ITextChannel>();
            channel.Setup(o => o.GetMessageAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(message.Object));

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.GetTextChannelAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(channel.Object));

            var client = new Mock<IDiscordClient>();
            client.Setup(o => o.GetGuildAsync(It.IsAny<ulong>(), It.IsAny<CacheMode>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(guild.Object));

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Client).Returns(client.Object);
            context.Setup(o => o.Channel).Returns(channel.Object);
            context.Setup(o => o.Guild).Returns(guild.Object);
            context.Setup(o => o.Message).Returns(new Mock<IUserMessage>().Object);

            var reader = new MessageTypeReader();
            var result = reader.ReadAsync(context.Object, "12345", null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);

            var msg = result.Values.First().Value as IMessage;
            Assert.AreEqual((ulong)12345, msg.Id);
        }
    }
}
