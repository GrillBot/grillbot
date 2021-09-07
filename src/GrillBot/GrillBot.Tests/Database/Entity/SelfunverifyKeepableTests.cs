using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class SelfunverifyKeepableTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new SelfunverifyKeepable(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var keepable = new SelfunverifyKeepable()
            {
                GroupName = "Group",
                Name = "Name"
            };

            TestHelpers.CheckDefaultPropertyValues(keepable, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
