using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void BirthdayAcceptYear_True()
        {
            var user = new User() { Birthday = new DateTime(1996, 4, 16) };
            Assert.IsTrue(user.BirthdayAcceptYear);
        }

        [TestMethod]
        public void BirthdayAcceptYear_MissingBirthday_False()
        {
            var user = new User();
            Assert.IsFalse(user.BirthdayAcceptYear);
        }

        [TestMethod]
        public void BirthdayAcceptYear_WithBirthday_False()
        {
            var user = new User() { Birthday = new DateTime(1, 6, 3) };
            Assert.IsFalse(user.BirthdayAcceptYear);
        }

        [TestMethod]
        public void HaveFlags_Single_True()
        {
            var user = new User() { Flags = (int)UserFlags.BotAdmin };

            Assert.IsTrue(user.HaveFlags(UserFlags.BotAdmin));
        }

        [TestMethod]
        public void HaveFlags_Multiple_True()
        {
            var user = new User() { Flags = (int)(UserFlags.BotAdmin | UserFlags.WebAdmin) };

            Assert.IsTrue(user.HaveFlags(UserFlags.WebAdmin | UserFlags.BotAdmin));
        }

        [TestMethod]
        public void HaveFlags_Single_False()
        {
            var user = new User();

            Assert.IsFalse(user.HaveFlags(UserFlags.BotAdmin));
        }

        [TestMethod]
        public void HaveFlags_Multiple_False()
        {
            var user = new User();

            Assert.IsFalse(user.HaveFlags(UserFlags.BotAdmin | UserFlags.WebAdmin));
        }

        [TestMethod]
        public void FromDiscord_User()
        {
            var dcUser = DiscordHelpers.CreateUserMock(12345, "User");

            dcUser.Setup(o => o.IsBot).Returns(false);
            dcUser.Setup(o => o.IsWebhook).Returns(false);

            var user = User.FromDiscord(dcUser.Object);
            Assert.IsFalse(user.HaveFlags(UserFlags.NotUser));
        }

        [TestMethod]
        public void FromDiscord_Bot()
        {
            var dcUser = DiscordHelpers.CreateUserMock(12345, "User");

            dcUser.Setup(o => o.IsBot).Returns(true);
            dcUser.Setup(o => o.IsWebhook).Returns(false);

            var user = User.FromDiscord(dcUser.Object);
            Assert.IsTrue(user.HaveFlags(UserFlags.NotUser));
        }

        [TestMethod]
        public void FromDiscord_Webhook()
        {
            var dcUser = DiscordHelpers.CreateUserMock(12345, "User");

            dcUser.Setup(o => o.IsBot).Returns(false);
            dcUser.Setup(o => o.IsWebhook).Returns(true);

            var user = User.FromDiscord(dcUser.Object);
            Assert.IsTrue(user.HaveFlags(UserFlags.NotUser));
        }

        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new User(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "Guilds":
                    case "UsedEmotes":
                    case "IncomingReminders":
                    case "OutgoingReminders":
                    case "Channels":
                    case "SearchItems":
                        Assert.AreNotEqual(defaultValue, value);
                        break;
                    default:
                        Assert.AreEqual(defaultValue, value);
                        break;
                }
            });
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var user = new User()
            {
                ApiToken = Guid.NewGuid(),
                Birthday = new DateTime(2021, 09, 09),
                Flags = 1,
                Id = "ABCD",
                Note = "Note",
                Username = "Username",
                SelfUnverifyMinimalTime = TimeSpan.FromSeconds(50)
            };

            TestHelpers.CheckNonDefaultPropertyValues(user);
        }
    }
}
