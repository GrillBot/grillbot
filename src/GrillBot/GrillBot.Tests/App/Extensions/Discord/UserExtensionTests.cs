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
    }
}
