using Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Modules.Implementations.Emotes;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Implementations.Emotes
{
    [TestClass]
    public class EmoteListExtensionsTests
    {
        [TestMethod]
        public void WithEmoteList_WithoutData_WithoutUser()
        {
            var data = new List<EmoteStatItem>();

            var user = DiscordHelpers.CreateUserMock(0, "Test");
            user.Setup(o => o.DiscriminatorValue).Returns(9982);
            user.Setup(o => o.Discriminator).Returns("9982");
            user.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns(null as string);
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/avatar.png");

            var embed = new EmbedBuilder()
                .WithEmoteList(data, user.Object, null, false, false, "count", 0);

            Assert.IsNotNull(embed.Footer);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Footer.Text));
            Assert.IsFalse(string.IsNullOrEmpty(embed.Footer.IconUrl));
            Assert.IsNotNull(embed.Author);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Author.Name));
            Assert.IsTrue(string.IsNullOrEmpty(embed.Author.IconUrl));
            Assert.AreEqual(Color.Blue, embed.Color);
            Assert.IsNotNull(embed.Timestamp);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Description));
        }

        [TestMethod]
        public void WithEmoteList_WithData_WithoutUser()
        {
            var data = new List<EmoteStatItem>()
            {
                new()
                {
                    LastOccurence = DateTime.MaxValue,
                    FirstOccurence = DateTime.MinValue,
                    UsersCount = 10,
                    UseCount = 50,
                    Id = "<:rtzW:567039874452946961>"
                }
            };

            var user = DiscordHelpers.CreateUserMock(0, "Test");
            user.Setup(o => o.DiscriminatorValue).Returns(9982);
            user.Setup(o => o.Discriminator).Returns("9982");
            user.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns(null as string);
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/avatar.png");

            var embed = new EmbedBuilder()
                .WithEmoteList(data, user.Object, null, false, false, "count", 0);

            Assert.IsNotNull(embed.Footer);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Footer.Text));
            Assert.IsFalse(string.IsNullOrEmpty(embed.Footer.IconUrl));
            Assert.IsNotNull(embed.Author);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Author.Name));
            Assert.IsTrue(string.IsNullOrEmpty(embed.Author.IconUrl));
            Assert.AreEqual(Color.Blue, embed.Color);
            Assert.IsNotNull(embed.Timestamp);
            Assert.IsTrue(string.IsNullOrEmpty(embed.Description));
            Assert.AreEqual(1, embed.Fields.Count);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Fields[0].Name));
            Assert.IsFalse(string.IsNullOrEmpty(embed.Fields[0].Value as string));
            Assert.IsTrue(embed.Fields[0].IsInline);
        }

        [TestMethod]
        public void WithEmoteList_WithoutData_WithUser()
        {
            var data = new List<EmoteStatItem>();

            var user = DiscordHelpers.CreateUserMock(0, "Test");
            user.Setup(o => o.DiscriminatorValue).Returns(9982);
            user.Setup(o => o.Discriminator).Returns("9982");
            user.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns(null as string);
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/avatar.png");

            var forUser = DiscordHelpers.CreateUserMock(12345, "");
            var embed = new EmbedBuilder()
                .WithEmoteList(data, user.Object, forUser.Object, false, false, "count", 0);

            Assert.IsNotNull(embed.Footer);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Footer.Text));
            Assert.IsFalse(string.IsNullOrEmpty(embed.Footer.IconUrl));
            Assert.IsNotNull(embed.Author);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Author.Name));
            Assert.IsTrue(string.IsNullOrEmpty(embed.Author.IconUrl));
            Assert.AreEqual(Color.Blue, embed.Color);
            Assert.IsNotNull(embed.Timestamp);
            Assert.IsFalse(string.IsNullOrEmpty(embed.Description));
        }
    }
}
