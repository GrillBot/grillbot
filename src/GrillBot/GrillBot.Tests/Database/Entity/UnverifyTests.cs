using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class UnverifyTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new Unverify());
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var unverify = new Unverify()
            {
                Channels = new List<GuildChannelOverride>(),
                EndAt = DateTime.Now,
                Guild = new Guild(),
                GuildId = "ABCD",
                GuildUser = new GuildUser(),
                Reason = "Prostě",
                Roles = new List<string>(),
                SetOperationId = 12345,
                StartAt = DateTime.MaxValue,
                UnverifyLog = new UnverifyLog(),
                UserId = "ABCD"
            };

            TestHelpers.CheckNonDefaultPropertyValues(unverify);
        }
    }
}
