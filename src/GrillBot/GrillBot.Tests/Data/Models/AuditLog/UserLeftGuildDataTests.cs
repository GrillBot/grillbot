using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(123);
            user.Setup(o => o.Username).Returns("User");

            var data = new UserLeftGuildData(50, false, "No", user.Object);

            Assert.AreEqual(50, data.MemberCount);
            Assert.IsFalse(data.IsBan);
            Assert.AreEqual("No", data.BanReason);
            Assert.IsNotNull(data.User);
        }
    }
}
