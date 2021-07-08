using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class UserJoinedAuditDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.AreEqual(0, new UserJoinedAuditData().MemberCount);
        }

        [TestMethod]
        public void Constructor()
        {
            Assert.AreEqual(10, new UserJoinedAuditData(10).MemberCount);
        }
    }
}
