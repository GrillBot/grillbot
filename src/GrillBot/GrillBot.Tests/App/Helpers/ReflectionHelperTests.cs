using GrillBot.App.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrillBot.Tests.App.Helpers
{
    [TestClass]
    public class ReflectionHelperTests
    {
        [TestMethod]
        public void GetAllInternalServices()
        {
            var handlers = ReflectionHelper.GetAllInternalServices();
            Assert.IsTrue(handlers.Any());
        }

        [TestMethod]
        public void GetAllReactionEventHandlers()
        {
            var handlers = ReflectionHelper.GetAllReactionEventHandlers();
            Assert.IsTrue(handlers.Any());
        }
    }
}
