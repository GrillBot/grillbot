using Discord;
using GrillBot.Data.Enums;
using GrillBot.Data.Models.Guilds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.Guilds
{
    [TestClass]
    public class UselessPermissionTests
    {
        [TestMethod]
        public void Constructor()
        {
            var channel = new Mock<IGuildChannel>();
            var user = new Mock<IGuildUser>();

            var perm = new UselessPermission(channel.Object, user.Object, UselessPermissionType.AvailableFromRole);
            TestHelpers.CheckNonDefaultPropertyValues(perm);
        }
    }
}
