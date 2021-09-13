using GrillBot.App.Extensions;
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
    }
}
