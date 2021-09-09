using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class GuildChannelOverrideTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new GuildChannelOverride());
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var @override = new GuildChannelOverride()
            {
                AllowValue = 1024,
                ChannelId = 1024,
                DenyValue = 1024
            };

            TestHelpers.CheckNonDefaultPropertyValues(@override);
        }
    }
}
