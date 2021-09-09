using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class GuildUserChannelTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildUserChannel());
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var channel = new GuildUserChannel()
            {
                GuildId = "Guild",
                Guild = new(),
                Channel = new(),
                Count = 42,
                FirstMessageAt = DateTime.MaxValue,
                Id = "Id",
                LastMessageAt = DateTime.MaxValue,
                User = new(),
                UserId = "User"
            };

            TestHelpers.CheckNonDefaultPropertyValues(channel);
        }
    }
}
