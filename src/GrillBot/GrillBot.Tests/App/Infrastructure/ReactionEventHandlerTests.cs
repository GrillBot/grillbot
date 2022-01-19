using Discord;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Data;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Infrastructure
{
    [TestClass]
    public class ReactionEventHandlerTests
    {
        class Handler : ReactionEventHandler
        {
            public bool TryGetEmbed<TMetadata>(IUserMessage message, IEmote reaction, out IEmbed embed, out TMetadata metadata) where TMetadata : IEmbedMetadata, new()
                => TryGetEmbedAndMetadata<TMetadata>(message, reaction, out embed, out metadata);

            public int GetPage(int current, int maxPages, IEmote emote)
                => GetPageNumber(current, maxPages, emote);
        }

        [TestMethod]
        public void TryGetEmbedAndMetadata_MissingEmbed()
        {
            var message = new Mock<IUserMessage>();
            message.Setup(o => o.Embeds).Returns(new List<IEmbed>().AsReadOnly());

            var handler = new Handler();
            var result = handler.TryGetEmbed<EmbedMetadata>(message.Object, null, out var _, out var __);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGetEmbedAndMetadata_MissingEmbedFooter()
        {
            var handler = new Handler();
            var message = new Mock<IUserMessage>();
            var embed = new EmbedBuilder().Build();
            var embeds = new List<IEmbed>() { embed };
            message.Setup(o => o.Embeds).Returns(embeds.AsReadOnly());

            var result = handler.TryGetEmbed<EmbedMetadata>(message.Object, null, out var _, out var __);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGetEmbedAndMetadata_MissingEmbedAuthor()
        {
            var handler = new Handler();
            var message = new Mock<IUserMessage>();
            var embed = new EmbedBuilder().WithFooter("Test", "http://test.com").Build();
            var embeds = new List<IEmbed>() { embed };
            message.Setup(o => o.Embeds).Returns(embeds.AsReadOnly());

            var result = handler.TryGetEmbed<EmbedMetadata>(message.Object, null, out var _, out var __);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGetEmbedAndMetadata_InvalidPaginationEmoji()
        {
            var handler = new Handler();
            var message = new Mock<IUserMessage>();
            var embed = new EmbedBuilder().WithAuthor("Test", "http://test.com", "http://test.com").WithFooter("Test", "http://test.com").Build();
            var embeds = new List<IEmbed>() { embed };
            message.Setup(o => o.Embeds).Returns(embeds.AsReadOnly());

            var result = handler.TryGetEmbed<EmbedMetadata>(message.Object, Emojis.LetterY, out var _, out var __);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGetEmbedAndMetadata_MissingReference()
        {
            var handler = new Handler();
            var message = new Mock<IUserMessage>();
            var embed = new EmbedBuilder().WithAuthor("Test", "http://test.com", "http://test.com").WithFooter("Test", "http://test.com").Build();
            var embeds = new List<IEmbed>() { embed };
            message.Setup(o => o.Embeds).Returns(embeds.AsReadOnly());

            var result = handler.TryGetEmbed<EmbedMetadata>(message.Object, Emojis.MoveToFirst, out var _, out var __);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryGetEmbedAndMetadata_InvalidParsing()
        {
            var handler = new Handler();
            var message = new Mock<IUserMessage>();
            var embed = new EmbedBuilder().WithAuthor("Test", "http://test.com", "http://test.com").WithFooter("Test", "http://test.com").Build();
            var embeds = new List<IEmbed>() { embed };
            message.Setup(o => o.Embeds).Returns(embeds.AsReadOnly());
            message.Setup(o => o.ReferencedMessage).Returns(new Mock<IUserMessage>().Object);

            var result = handler.TryGetEmbed<EmbedMetadata>(message.Object, Emojis.MoveToFirst, out var _, out var __);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetPageNumber()
        {
            var cases = new List<Tuple<int, int, IEmote, int>>()
            {
                new Tuple<int, int, IEmote, int>(3, 30, Emojis.MoveToFirst, 0),
                new Tuple<int, int, IEmote, int>(30, 30, Emojis.MoveToLast, 29),
                new Tuple<int, int, IEmote, int>(3, 30, Emojis.MoveToNext, 4),
                new Tuple<int, int, IEmote, int>(3, 30, Emojis.MoveToPrev, 2)
            };

            var handler = new Handler();
            foreach (var @case in cases)
            {
                var result = handler.GetPage(@case.Item1, @case.Item2, @case.Item3);
                Assert.AreEqual(@case.Item4, result);
            }
        }

        [TestMethod]
        public void GetPageNumber_OverMaxPages()
        {
            var handler = new Handler();
            var result = handler.GetPage(29, 30, Emojis.MoveToNext);
            Assert.AreEqual(29, result);
        }

        [TestMethod]
        public void GetPageNumber_LowerThanZero()
        {
            var handler = new Handler();
            var result = handler.GetPage(0, 30, Emojis.MoveToPrev);
            Assert.AreEqual(0, result);
        }
    }
}
