using GrillBot.Data.Models.API.Emotes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Emotes
{
    [TestClass]
    public class EmoteStatItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new EmoteStatItem(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }
    }
}
