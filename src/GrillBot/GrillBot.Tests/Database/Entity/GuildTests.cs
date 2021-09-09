using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class GuildTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new Guild(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "Users":
                    case "Invites":
                    case "Channels":
                    case "Searches":
                    case "Unverifies":
                    case "UnverifyLogs":
                    case "AuditLogs":
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
            var guild = new Guild()
            {
                AdminChannelId = "1234",
                BoosterRoleId = "11345",
                Id = "123456",
                MuteRoleId = "abcd",
                Name = "Channel"
            };

            TestHelpers.CheckNonDefaultPropertyValues(guild);
        }
    }
}
