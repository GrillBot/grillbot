using Discord;
using GrillBot.Data.Models.API.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.API.Users
{
    [TestClass]
    public class GuildUserDetailTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildUserDetail());
        }

        [TestMethod]
        public void FilledConstructor_WithInvites()
        {
            var entity = new GrillBot.Database.Entity.GuildUser()
            {
                Guild = new(),
                UsedInvite = new() { Creator = new() { User = new() } },
                Points = 50,
                GivenReactions = 50,
                ObtainedReactions = 50,
                Nickname = "Nickname"
            };

            entity.CreatedInvites.Add(new());
            entity.Channels.Add(new() { Channel = new() { Name = "Channel" } });

            var guild = new Mock<IGuild>();

            var detail = new GuildUserDetail(entity, guild.Object);
            TestHelpers.CheckNonDefaultPropertyValues(detail);
        }

        [TestMethod]
        public void FilledConstructor_WithoutInvites()
        {
            var entity = new GrillBot.Database.Entity.GuildUser()
            {
                Guild = new(),
                Points = 50,
                GivenReactions = 50,
                ObtainedReactions = 50,
                Nickname = "Nickname"
            };

            entity.Channels.Add(new() { Channel = new() { Name = "Channel" } });
            var guild = new Mock<IGuild>();

            var detail = new GuildUserDetail(entity, guild.Object);
            TestHelpers.CheckDefaultPropertyValues(detail, (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "UsedInvite":
                        Assert.AreEqual(defaultValue, value);
                        return;
                    default:
                        Assert.AreNotEqual(defaultValue, value);
                        break;
                }
            });
        }
    }
}
