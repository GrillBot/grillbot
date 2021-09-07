using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class GuildUserTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildUser(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "CreatedInvites":
                    case "Channels":
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
            var user = new GuildUser()
            {
                UserId = "User",
                User = new(),
                GivenReactions = 42,
                Guild = new(),
                GuildId = "Guild",
                LastPointsMessageIncrement = DateTime.MaxValue,
                LastPointsReactionIncrement = DateTime.MaxValue,
                Nickname = "Nickname",
                ObtainedReactions = 50,
                Points = 10000,
                Unverify = new(),
                UsedInvite = new(),
                UsedInviteCode = "Code"
            };

            TestHelpers.CheckDefaultPropertyValues(user, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
