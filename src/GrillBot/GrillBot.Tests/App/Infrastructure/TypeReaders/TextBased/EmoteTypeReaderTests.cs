using Discord;
using Discord.Commands;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.TypeReaders.TextBased;
using GrillBot.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders.TextBased
{
    [TestClass]
    public class EmoteTypeReaderTests
    {
        [TestMethod]
        public void Read_Emoji()
        {
            var emoji = Emojis.Ok.ToString();

            var reader = new EmotesTypeReader();
            var result = reader.ReadAsync(null, emoji, null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.IsTrue(Emojis.Ok.IsEqual(result.Values.First().Value as Emoji));
        }

        [TestMethod]
        public void Read_Emote()
        {
            const string rtzW = "<:rtzW:567039874452946961>";

            var reader = new EmotesTypeReader();
            var result = reader.ReadAsync(null, rtzW, null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(rtzW, ((Emote)result.Values.First().Value).ToString());
        }

        [TestMethod]
        public void Read_NotFound()
        {
            var guild = new Mock<IGuild>();
            guild.Setup(o => o.GetEmoteAsync(It.IsAny<ulong>(), It.IsAny<RequestOptions>())).Returns(Task.FromResult(null as GuildEmote));
            guild.Setup(o => o.Emotes).Returns(new List<GuildEmote>().AsReadOnly());

            var context = new Mock<ICommandContext>();
            context.Setup(o => o.Guild).Returns(guild.Object);

            var reader = new EmotesTypeReader();
            var result = reader.ReadAsync(context.Object, "567039874452946961", null).Result;

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.Values);
        }
    }
}
