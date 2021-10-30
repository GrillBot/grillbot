using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class GuildChannelTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildChannel(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "SearchItems":
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
            var channel = new GuildChannel()
            {
                ChannelId = "Channel",
                ChannelType = Discord.ChannelType.Category,
                Guild = new(),
                GuildId = "Guild",
                Name = "Name",
            };

            TestHelpers.CheckNonDefaultPropertyValues(channel);
        }
    }
}
