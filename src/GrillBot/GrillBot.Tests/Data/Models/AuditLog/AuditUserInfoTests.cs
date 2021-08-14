using Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class AuditUserInfoTests
    {
        [TestMethod]
        public void Constructor()
        {
            var user = DiscordHelpers.CreateUserMock(12345, "User");
            user.Setup(o => o.Discriminator).Returns("0000");

            var auditUser = new AuditUserInfo(user.Object);

            Assert.AreEqual((ulong)12345, auditUser.Id);
            Assert.AreEqual("User", auditUser.Username);
            Assert.AreEqual("0000", auditUser.Discriminator);
        }

        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.AreEqual((ulong)0, new AuditUserInfo().Id);
        }

        [TestMethod]
        public void CompareTo_False()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            Assert.AreEqual(1, auditUser.CompareTo(new AuditUserInfo()));
            Assert.AreEqual(1, auditUser.CompareTo(new object()));
        }

        [TestMethod]
        public void CompareTo_True()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            Assert.AreEqual(0, auditUser.CompareTo(new AuditUserInfo(user.Object)));
        }

        [TestMethod]
        public void Equals_True()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            Assert.IsTrue(auditUser.Equals(new AuditUserInfo(user.Object)));
        }

        [TestMethod]
        public void Equals_False()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            Assert.IsFalse(auditUser.Equals(new AuditUserInfo()));
            Assert.IsFalse(auditUser.Equals(new object()));
        }

        [TestMethod]
        public void Operators_True()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            var anotherUser = new AuditUserInfo(user.Object);
            var emptyUser = new AuditUserInfo();

            Assert.IsTrue(auditUser == anotherUser);
            Assert.IsTrue(auditUser != emptyUser);
            Assert.IsTrue(auditUser < emptyUser);
            Assert.IsTrue(auditUser > emptyUser);
            Assert.IsTrue(auditUser <= emptyUser);
            Assert.IsTrue(auditUser >= emptyUser);
        }

        [TestMethod]
        public void Operators_False()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            var anotherUser = new AuditUserInfo(user.Object);
            var emptyUser = new AuditUserInfo();

            Assert.IsFalse(auditUser == emptyUser);
            Assert.IsFalse(auditUser != anotherUser);
            Assert.IsFalse(auditUser < anotherUser);
            Assert.IsFalse(auditUser > anotherUser);
            Assert.IsFalse(auditUser <= anotherUser);
            Assert.IsFalse(auditUser >= anotherUser);
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            var user = DiscordHelpers.CreateUserMock(12345, null);

            var auditUser = new AuditUserInfo(user.Object);
            Assert.IsTrue(auditUser.GetHashCode() != 0);
        }

        [TestMethod]
        public void ToStringTest_WithoutDiscriminator()
        {
            var user = DiscordHelpers.CreateUserMock(12345, "User");

            var auditUser = new AuditUserInfo(user.Object);
            Assert.AreEqual("User", auditUser.ToString());
        }

        [TestMethod]
        public void ToStringTest_WithDiscriminator()
        {
            var user = DiscordHelpers.CreateUserMock(12345, "User");
            user.Setup(o => o.Discriminator).Returns("0000");

            var auditUser = new AuditUserInfo(user.Object);
            Assert.AreEqual("User#0000", auditUser.ToString());
        }
    }
}
