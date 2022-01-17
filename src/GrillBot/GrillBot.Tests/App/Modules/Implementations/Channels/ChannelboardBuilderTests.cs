using Discord;
using GrillBot.Data.Modules.Implementations.Channels;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace GrillBot.Tests.App.Modules.Implementations.Channels
{
    [TestClass]
    public class ChannelboardBuilderTests
    {
        [TestMethod]
        public void WithChannelboard()
        {
            var user = DiscordHelpers.CreateUserMock(0, "Test");
            user.Setup(o => o.DiscriminatorValue).Returns(9982);
            user.Setup(o => o.Discriminator).Returns("9982");
            user.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns(null as string);
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/avatar.png");

            var guild = new Mock<IGuild>();
            guild.Setup(o => o.Id).Returns(1234);

            var channel = new Mock<ITextChannel>();
            channel.Setup(o => o.Name).Returns("Channel");

            var data = new List<KeyValuePair<string, long>>()
            {
                new KeyValuePair<string, long>("1234", 50)
            };

            var embed = new ChannelboardBuilder()
                .WithChannelboard(user.Object, guild.Object, data, _ => channel.Object, 0, 0);

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
