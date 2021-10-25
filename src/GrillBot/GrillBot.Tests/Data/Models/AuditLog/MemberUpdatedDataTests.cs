using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class MemberUpdatedDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new MemberUpdatedData(new AuditUserInfo()).Nickname);
        }

        [TestMethod]
        public void Constructor_GuildUsers()
        {
            var user = DiscordHelpers.CreateGuildUserMock(0, null, "User");
            user.Setup(o => o.IsMuted).Returns(false);
            user.Setup(o => o.IsDeafened).Returns(true);

            var data = new MemberUpdatedData(user.Object, user.Object);

            Assert.IsTrue(data.Nickname.IsEmpty);
            Assert.IsTrue(data.IsMuted.IsEmpty);
            Assert.IsTrue(data.IsDeaf.IsEmpty);
            Assert.IsFalse(data.Roles.Count > 0);
        }

        [TestMethod]
        public void Initializer()
        {
            _ = new MemberUpdatedData(new AuditUserInfo())
            {
                Roles = new List<AuditRoleUpdateInfo>()
            };

            Assert.IsTrue(true);
        }
    }
}
