using GrillBot.Data.Models.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API
{
    [TestClass]
    public class RoleTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new Role(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }
    }
}
