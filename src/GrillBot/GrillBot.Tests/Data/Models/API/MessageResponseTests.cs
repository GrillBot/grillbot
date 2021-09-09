using GrillBot.Data.Models.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API
{
    [TestClass]
    public class MessageResponseTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new MessageResponse());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var response = new MessageResponse("Hello");
            Assert.AreEqual("Hello", response.Message);
        }
    }
}
