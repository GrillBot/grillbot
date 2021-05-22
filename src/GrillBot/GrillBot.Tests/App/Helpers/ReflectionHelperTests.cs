using GrillBot.App.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrillBot.Tests.App.Helpers
{
    [TestClass]
    public class ReflectionHelperTests
    {
        [TestMethod]
        public void GetAllEventHandlers()
        {
            var handlers = ReflectionHelper.GetAllEventHandlers();
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
