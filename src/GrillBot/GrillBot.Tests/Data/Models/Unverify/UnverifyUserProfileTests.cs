using Discord;
using GrillBot.Data.Models.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace GrillBot.Tests.Data.Models.Unverify
{
    [TestClass]
    public class UnverifyUserProfileTests
    {
        [TestMethod]
        public void ReturnRoles()
        {
            var user = new Mock<IGuildUser>();
            var role = new Mock<IRole>();

            var profile = new UnverifyUserProfile(user.Object, DateTime.MinValue, DateTime.MaxValue, false)
            {
                RolesToRemove = new List<IRole>() { role.Object }
            };

            profile.ReturnRolesAsync().Wait();
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void RemoveRoles()
        {
            var user = new Mock<IGuildUser>();
            var role = new Mock<IRole>();

            var profile = new UnverifyUserProfile(user.Object, DateTime.MinValue, DateTime.MaxValue, false)
            {
                RolesToRemove = new List<IRole>() { role.Object }
            };

            profile.RemoveRolesAsync().Wait();
            Assert.IsTrue(true);
        }
    }
}
