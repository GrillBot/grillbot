using GrillBot.Data.Services.Unverify;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Services.Unverify
{
    [TestClass]
    public class UnverifyExtensionsTests
    {
        [TestMethod]
        public void AddUnverify()
        {
            var collection = new ServiceCollection().AddUnverify();

            Assert.AreEqual(5, collection.Count);
        }
    }
}
