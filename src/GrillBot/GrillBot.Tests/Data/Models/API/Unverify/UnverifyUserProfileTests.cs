using Discord;
using GrillBot.Data.Models.API.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyUserProfileTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyUserProfile(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var user = new Mock<IGuildUser>();
            user.Setup(o => o.Id).Returns(12345);
            user.Setup(o => o.Username).Returns("Username");
            user.Setup(o => o.Discriminator).Returns("12345");
            user.Setup(o => o.IsBot).Returns(false);
            user.Setup(o => o.GetAvatarUrl(It.IsAny<ImageFormat>(), It.IsAny<ushort>())).Returns(null as string);
            user.Setup(o => o.GetDefaultAvatarUrl()).Returns("http://discord.com/image.png");

            var role = new Mock<IRole>();

            var profile = new GrillBot.Data.Models.Unverify.UnverifyUserProfile(user.Object, DateTime.Now, DateTime.MaxValue, true)
            {
                Reason = "Prostě důvod"
            };
            profile.RolesToKeep.Add(role.Object);
            profile.RolesToRemove.Add(role.Object);

            var userProfile = new UnverifyUserProfile(profile);
            TestHelpers.CheckDefaultPropertyValues(userProfile, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
