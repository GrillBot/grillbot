using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class InviteTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new Invite(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "UsedUsers":
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
            var invite = new Invite()
            {
                Code = "Code",
                CreatedAt = DateTime.MaxValue,
                Creator = new(),
                CreatorId = "Creator",
                Guild = new(),
                GuildId = "Guild"
            };

            TestHelpers.CheckDefaultPropertyValues(invite, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
