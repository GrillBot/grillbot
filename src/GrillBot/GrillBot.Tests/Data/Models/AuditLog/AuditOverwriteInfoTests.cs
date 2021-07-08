using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class AuditOverwriteInfoTests
    {
        [TestMethod]
        public void Constructor()
        {
            var overwrite = new Overwrite(12345, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow, manageChannel: PermValue.Deny));
            var info = new AuditOverwriteInfo(overwrite);

            Assert.AreEqual((ulong)12345, info.TargetId);
            Assert.AreEqual(PermissionTarget.Role, info.Target);
            Assert.AreNotEqual(0, info.AllowValue);
            Assert.AreNotEqual(0, info.DenyValue);
        }

        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.AreEqual((ulong)0, new AuditOverwriteInfo().TargetId);
        }
    }
}
