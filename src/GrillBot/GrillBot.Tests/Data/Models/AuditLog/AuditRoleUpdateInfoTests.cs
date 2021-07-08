using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class AuditRoleUpdateInfoTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsFalse(new AuditRoleUpdateInfo().Added);
        }

        [TestMethod]
        public void Constructor()
        {
            var role = new Mock<IRole>();
            role.Setup(o => o.Id).Returns(123);
            role.Setup(o => o.Name).Returns("Role");
            role.Setup(o => o.Color).Returns(Color.Red);

            var info = new AuditRoleUpdateInfo(role.Object, true);

            Assert.AreEqual((ulong)123, info.Id);
            Assert.AreEqual("Role", info.Name);
            Assert.AreEqual(Color.Red.RawValue, info.Color);
            Assert.IsTrue(info.Added);
        }
    }
}
