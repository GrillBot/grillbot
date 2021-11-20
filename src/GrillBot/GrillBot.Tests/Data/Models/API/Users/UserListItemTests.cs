using Discord;
using Discord.WebSocket;
using GrillBot.Data.Models.API.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.Data.Models.API.Users
{
    [TestClass]
    public class UserListItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UserListItem());
        }

        [TestMethod]
        public void EmptyConstructor_Filled()
        {
            var userListItem = new UserListItem()
            {
                DiscordStatus = Discord.UserStatus.Idle,
                Flags = 50,
                Guilds = new Dictionary<string, bool>() { { "A", true } },
                HaveBirthday = true,
                Id = "12345",
                Username = "Test"
            };

            TestHelpers.CheckNonDefaultPropertyValues(userListItem);
        }

        [TestMethod]
        public void FilledConstructor_WithBirthday()
        {
            var entity = new GrillBot.Database.Entity.User()
            {
                Id = "12345",
                Birthday = new DateTime(2021, 11, 20),
                Flags = 0,
                Username = "Test"
            };

            var item = new UserListItem(entity, null, null);
            Assert.IsTrue(item.HaveBirthday);
        }

        [TestMethod]
        public void FilledConstructor_WithDiscordUser()
        {
            var entity = new GrillBot.Database.Entity.User()
            {
                Id = "12345",
                Flags = 0,
                Username = "Test"
            };

            var dcUser = new Mock<IUser>();
            dcUser.Setup(o => o.Discriminator).Returns("1234");
            dcUser.Setup(o => o.Status).Returns(UserStatus.Online);

            var item = new UserListItem(entity, null, dcUser.Object);
            Assert.AreEqual(UserStatus.Online, item.DiscordStatus);
        }

        [TestMethod]
        public void FilledConstructor_Guilds()
        {
            var entity = new GrillBot.Database.Entity.User()
            {
                Id = "12345",
                Flags = 0,
                Username = "Test"
            };

            entity.Guilds.Add(new GrillBot.Database.Entity.GuildUser()
            {
                GuildId = "12345",
                UserId = "12345",
                Guild = new GrillBot.Database.Entity.Guild()
                {
                    Name = "Test"
                }
            });

            var dcUser = new Mock<IUser>();
            dcUser.Setup(o => o.Discriminator).Returns("1234");
            dcUser.Setup(o => o.Status).Returns(UserStatus.Online);

            var item = new UserListItem(entity, new DiscordSocketClient(), dcUser.Object);
            Assert.AreEqual(UserStatus.Online, item.DiscordStatus);
        }
    }
}
