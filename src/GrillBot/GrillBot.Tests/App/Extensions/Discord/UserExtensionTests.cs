using Discord;
using GrillBot.App.Extensions.Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Extensions.Discord
{
    [TestClass]
    public class UserExtensionTests
    {
        [TestMethod]
        public void HaveAnimatedAvatar_True()
        {
            var mock = new Mock<IUser>();
            mock.Setup(o => o.AvatarId).Returns("a_asdf");
            var user = mock.Object;

            Assert.IsTrue(user.HaveAnimatedAvatar());
        }

        [TestMethod]
        public void HaveAnimatedAvatar_False()
        {
            var mock = new Mock<IUser>();
            mock.Setup(o => o.AvatarId).Returns("asdf");
            var user = mock.Object;

            Assert.IsFalse(user.HaveAnimatedAvatar());
        }

        [TestMethod]
        public void IsUser_Webhook_False()
        {
            var mock = new Mock<IUser>();

            mock.Setup(o => o.IsBot).Returns(false);
            mock.Setup(o => o.IsWebhook).Returns(true);

            Assert.IsFalse(mock.Object.IsUser());
        }

        [TestMethod]
        public void IsUser_Bot_False()
        {
            var mock = new Mock<IUser>();

            mock.Setup(o => o.IsBot).Returns(true);
            mock.Setup(o => o.IsWebhook).Returns(false);

            Assert.IsFalse(mock.Object.IsUser());
        }

        [TestMethod]
        public void IsUser_User_True()
        {
            var mock = new Mock<IUser>();

            mock.Setup(o => o.IsBot).Returns(false);
            mock.Setup(o => o.IsWebhook).Returns(false);

            Assert.IsTrue(mock.Object.IsUser());
        }

        [TestMethod]
        public void GetAvatarUri_Default()
        {
            var mock = new Mock<IUser>();

            mock.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns((string)null);
            mock.Setup(o => o.GetDefaultAvatarUrl()).Returns("https://discord.com");

            var result = mock.Object.GetAvatarUri();
            Assert.AreEqual("https://discord.com", result);
        }

        [TestMethod]
        public void GetAvatarUri_User()
        {
            var mock = new Mock<IUser>();

            mock.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns("https://discord.com/user.jpg");
            mock.Setup(o => o.GetDefaultAvatarUrl()).Returns("https://discord.com");

            var result = mock.Object.GetAvatarUri();
            Assert.AreEqual("https://discord.com/user.jpg", result);
        }

        [TestMethod]
        public void DownloadAvatar_Async()
        {
            var mock = new Mock<IUser>();
            mock.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns("https://www.google.cz/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png");

            mock.Object.DownloadAvatarAsync().ContinueWith(data =>
            {
                Assert.IsNotNull(data);
                Assert.IsNotNull(data.Result);
                Assert.IsTrue(data.Result.Length > 0);
            });
        }

        [TestMethod]
        public void GetDisplayName_GuildUser_WithoutNick()
        {
            var mock = new Mock<IGuildUser>();
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void GetDisplayName_GuildUser_WithNick()
        {
            var mock = new Mock<IGuildUser>();
            mock.Setup(o => o.Nickname).Returns("Testik");
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName();
            Assert.AreEqual("Testik", result);
        }

        [TestMethod]
        public void GetDisplayName_BasicUser()
        {
            var mock = new Mock<IUser>();
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void GetDisplayName_BasicUser_WithoutDiscriminator()
        {
            var mock = new Mock<IUser>();
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetDisplayName(false);
            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public void GetFullName_GuildUser_WithoutNick()
        {
            var mock = new Mock<IGuildUser>();
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetFullName();
            Assert.AreEqual("Test#1234", result);
        }

        [TestMethod]
        public void GetFullName_GuildUser_WithNick()
        {
            var mock = new Mock<IGuildUser>();
            mock.Setup(o => o.Nickname).Returns("Testik");
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetFullName();
            Assert.AreEqual("Testik (Test#1234)", result);
        }

        [TestMethod]
        public void GetFullName_BasicUser()
        {
            var mock = new Mock<IUser>();
            mock.Setup(o => o.Username).Returns("Test");
            mock.Setup(o => o.Discriminator).Returns("1234");

            var result = mock.Object.GetFullName();
            Assert.AreEqual("Test#1234", result);
        }
    }
}
