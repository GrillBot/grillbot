using Discord;
using Discord.WebSocket;
using GrillBot.Data.Modules.Implementations.Searching;
using GrillBot.Data.Models;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Implementations.Searching
{
    [TestClass]
    public class SearchingExtensionsTests
    {
        [TestMethod]
        public void WithSearching()
        {
            var data = new List<SearchingItem>()
            {
                new SearchingItem() { DisplayName = "User", Id = 1, JumpLink = "http://discord.com/jump/link", Message = "Hello" }
            };

            var channel = new Mock<ISocketMessageChannel>();
            channel.Setup(o => o.Id).Returns(12345);
            channel.Setup(o => o.Name).Returns("Kanal");

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(123456);

            var user = DiscordHelpers.CreateUserMock(0, "User");
            user.Setup(o => o.Discriminator).Returns("0000");
            user.Setup(o => o.AvatarId).Returns((string)null);
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/image.png");

            var embed = new EmbedBuilder()
                .WithSearching(data, channel.Object, guild.Object, 0, user.Object);

            Assert.AreEqual(1, embed.Fields.Count);
            Assert.IsNotNull(embed.Author);
            Assert.AreEqual(Color.Blue, embed.Color);
        }
    }
}
