using GrillBot.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class ClaimsPrincipalExtensionsTests
    {
        [TestMethod]
        public void GetUserId_NoUser()
        {
            var user = new ClaimsPrincipal();
            var userId = user.GetUserId();

            Assert.AreEqual((ulong)0, userId);
        }

        [TestMethod]
        public void GetUserId_User()
        {
            const ulong id = 12345;

            var user = new ClaimsPrincipal(new[]
            {
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, id.ToString())
                })
            });

            var userId = user.GetUserId();
            Assert.AreEqual(id, userId);
        }

        #region Permissions

        private static ClaimsPrincipal CreatePrincipal(string role)
        {
            return new ClaimsPrincipal(new[]
            {
                new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, role)
                })
            });
        }

        [TestMethod]
        public void HaveUserRole_True()
        {
            var user = CreatePrincipal("User");
            Assert.IsTrue(user.HaveUserPermission());
        }

        [TestMethod]
        public void HaveUserRole_False()
        {
            var user = CreatePrincipal("Admin");
            Assert.IsFalse(user.HaveUserPermission());
        }

        [TestMethod]
        public void HaveAdminPermission_True()
        {
            var user = CreatePrincipal("Admin");
            Assert.IsTrue(user.HaveAdminPermission());
        }

        [TestMethod]
        public void HaveAdminPermission_False()
        {
            var user = CreatePrincipal("User");
            Assert.IsFalse(user.HaveAdminPermission());
        }

        #endregion
    }
}
