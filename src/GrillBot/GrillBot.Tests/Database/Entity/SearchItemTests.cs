using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class SearchItemTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new SearchItem());
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var search = new SearchItem()
            {
                Channel = new(),
                UserId = "ABCD",
                ChannelId = "ABCD",
                Guild = new(),
                GuildId = "ABCD",
                Id = 42,
                MessageId = "ABCD",
                User = new(),
                JumpUrl = "JumpUrl",
                MessageContent = "Message"
            };

            TestHelpers.CheckNonDefaultPropertyValues(search);
        }
    }
}
