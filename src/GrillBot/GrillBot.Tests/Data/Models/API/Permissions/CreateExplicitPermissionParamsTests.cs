using GrillBot.Data.Models.API.Permissions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Permissions
{
    [TestClass]
    public class CreateExplicitPermissionParamsTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new CreateExplicitPermissionParams());
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            var parameters = new CreateExplicitPermissionParams()
            {
                Command = "Command",
                IsRole = true,
                TargetId = "Target"
            };

            TestHelpers.CheckNonDefaultPropertyValues(parameters);
        }
    }
}
