using Discord;
using GrillBot.App.Extensions.Discord;
using GrillBot.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.Data
{
    [TestClass]
    public class EmojisTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ConvertStringToEmoji_Invalid_Duplicity()
        {
            Emojis.ConvertStringToEmoji("tt");
        }

        [TestMethod]
        public void ConvertStringToEmoji()
        {
            var cases = new Dictionary<string, Tuple<Emoji[], bool>>()
            {
                { "A", new Tuple<Emoji[], bool>(new[] { Emojis.LetterA }, false) },
                { "AB", new Tuple<Emoji[], bool>(new[] { Emojis.LetterA, Emojis.LetterB }, false) },
                { "AA", new Tuple<Emoji[], bool>(new[] { Emojis.LetterA, Emojis.SignA }, true) },
                { "ABCDEFGHIJKLMNOPQRSTUVWXYZ", new Tuple<Emoji[], bool>(new[] { Emojis.LetterA, Emojis.LetterB, Emojis.LetterC, Emojis.LetterD, Emojis.LetterE, Emojis.LetterF, Emojis.LetterG, Emojis.LetterH, Emojis.LetterI, Emojis.LetterJ, Emojis.LetterK, Emojis.LetterL, Emojis.LetterM, Emojis.LetterN, Emojis.LetterO, Emojis.LetterP, Emojis.LetterQ, Emojis.LetterR, Emojis.LetterS, Emojis.LetterT, Emojis.LetterU, Emojis.LetterV, Emojis.LetterW, Emojis.LetterX, Emojis.LetterY, Emojis.LetterZ }, false) },
                { "123456789", new Tuple<Emoji[], bool>(new[]{ Emojis.One, Emojis.Two, Emojis.Three, Emojis.Four, Emojis.Five, Emojis.Six, Emojis.Seven, Emojis.Eight, Emojis.Nine }, false) }
            };

            foreach (var testCase in cases)
            {
                var result = Emojis.ConvertStringToEmoji(testCase.Key, testCase.Value.Item2);
                Assert.AreEqual(testCase.Value.Item1.Length, result.Count);

                foreach (var item in result)
                {
                    Assert.IsTrue(testCase.Value.Item1.Any(o => o.IsEqual(item)));
                }
            }
        }

        [TestMethod]
        public void EmojiToIntMap()
        {
            var keys = Emojis.EmojiToIntMap.Keys.ToArray();
            var values = Emojis.EmojiToIntMap.Values.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {
                Assert.AreEqual(values[i], Emojis.EmojiToIntMap[keys[i]]);
            }
        }

        [TestMethod]
        public void PaginationEmojis()
        {
            foreach (var emoji in Emojis.PaginationEmojis)
            {
                Assert.IsNotNull(emoji);
            }
        }

        [TestMethod]
        public void AnotherEmojis()
        {
            Assert.IsNotNull(Emojis.PersonRisingHand);
        }
    }
}
