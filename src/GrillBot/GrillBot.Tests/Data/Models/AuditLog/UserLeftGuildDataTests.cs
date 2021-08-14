using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class UserLeftGuildDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new UserLeftGuildData().User);
        }

        [TestMethod]
        public void Constructor()
        {
            var user = DiscordHelpers.CreateUserMock(123, "User");

            var data = new UserLeftGuildData(50, false, "No", user.Object);

            Assert.AreEqual(50, data.MemberCount);
            Assert.IsFalse(data.IsBan);
            Assert.AreEqual("No", data.BanReason);
            Assert.IsNotNull(data.User);
        }
    }
}
