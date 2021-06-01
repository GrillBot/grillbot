using Discord;
using GrillBot.App.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class EmoteExtensionTests
    {
        [TestMethod]
        public void CompareEmotes_True()
        {
            var emote = Emote.Parse("<:LP_feelsHighMan:604491522892890142>");
            Assert.IsTrue(emote.IsEqual(emote));
        }

        [TestMethod]
        public void CompareEmotes_False()
        {
            var emote = Emote.Parse("<:LP_feelsHighMan:604491522892890142>");
            var rightEmote = Emote.Parse("<a:aPES_EvilPlan:681576192977272874>");

            Assert.IsFalse(emote.IsEqual(rightEmote));
        }

        [TestMethod]
        public void CompareEmojis_True()
        {
            var emoji = new Emoji("✅");
            Assert.IsTrue(emoji.IsEqual(emoji));
        }

        [TestMethod]
        public void CompareEmojis_False()
        {
            var emoji = new Emoji("✅");
            var rightEmoji = new Emoji("❌");

            Assert.IsFalse(emoji.IsEqual(rightEmoji));
        }

        [TestMethod]
        public void CompareEmotes_InvalidTypes()
        {
            var emoji = new Emoji("✅");
            var rightEmote = Emote.Parse("<a:aPES_EvilPlan:681576192977272874>");

            Assert.IsFalse(emoji.IsEqual(rightEmote));
        }
    }
}
