using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class UnverifyLogTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLog());
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var logItem = new UnverifyLog()
            {
                CreatedAt = DateTime.Now,
                Data = "{}",
                FromUser = new GuildUser(),
                FromUserId = "ABCD",
                Guild = new Guild(),
                GuildId = "ABCD",
                Id = 42,
                Operation = UnverifyOperation.Selfunverify,
                ToUser = new GuildUser(),
                ToUserId = "ABCD",
                Unverify = new Unverify()
            };

            TestHelpers.CheckNonDefaultPropertyValues(logItem);
        }
    }
}
