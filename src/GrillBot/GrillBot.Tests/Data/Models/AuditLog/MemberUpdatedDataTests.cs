using Discord;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class MemberUpdatedDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new MemberUpdatedData().Nickname);
        }

        [TestMethod]
        public void Constructor_GuildUsers()
        {
            var user = new Mock<IGuildUser>();
            user.Setup(o => o.Nickname).Returns("User");
            user.Setup(o => o.IsMuted).Returns(false);
            user.Setup(o => o.IsDeafened).Returns(true);

            var data = new MemberUpdatedData(user.Object, user.Object);

            Assert.IsTrue(data.Nickname.IsEmpty);
            Assert.IsTrue(data.IsMuted.IsEmpty);
            Assert.IsTrue(data.IsDeaf.IsEmpty);
            Assert.IsNull(data.Roles);
        }

        [TestMethod]
        public void Initializer()
        {
            _ = new MemberUpdatedData()
            {
                Roles = new List<AuditRoleUpdateInfo>()
            };

            Assert.IsTrue(true);
        }
    }
}
