using Discord;
using GrillBot.App.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class EmbedExtensionsTests
    {
        [TestMethod]
        public void WithFooter()
        {
            var user = new Mock<IUser>();

            user.Setup(o => o.Username).Returns("GrillBot");
            user.Setup(o => o.Discriminator).Returns("1234");
            user.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns("https://discord.com");

            var embed = new EmbedBuilder()
                .WithFooter(user.Object)
                .Build();

            Assert.IsTrue(embed.Footer.HasValue);
            Assert.AreEqual("GrillBot#1234", embed.Footer.Value.Text);
            Assert.AreEqual("https://discord.com", embed.Footer.Value.IconUrl);
        }
    }
}
