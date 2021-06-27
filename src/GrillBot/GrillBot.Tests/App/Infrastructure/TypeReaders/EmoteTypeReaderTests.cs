using Discord;
using GrillBot.App.Extensions.Discord;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders
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
            Assert.AreEqual(rtzW, (result.Values.First().Value as Emote)?.ToString());
        }
    }
}
