using GrillBot.Data.Models.API.Permissions;
using GrillBot.Data.Models.API.Users;
using GrillBot.Database.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Permissions
{
    [TestClass]
    public class ExplicitPermissionTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new ExplicitPermission());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var permission = new ExplicitPermission(new() { Command = "Command", State = ExplicitPermissionState.Banned }, new(), new());
            TestHelpers.CheckNonDefaultPropertyValues(permission);
        }

        [TestMethod]
        public void FilledConstructor_WithoutUser()
        {
            var permission = new ExplicitPermission(new() { Command = "Command", State = ExplicitPermissionState.Banned }, null, new());
            TestHelpers.CheckNonDefaultPropertyValues(permission, (defaultValue, value, propertyName) => Assert.AreNotEqual(propertyName == "User" ? new User() : defaultValue, value));
        }
    }
}
