using GrillBot.App.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GrillBot.Tests.App.Services
{
    [TestClass]
    public class RandomizationServiceTests
    {
        [TestMethod]
        public void Next()
        {
            const string key = nameof(RandomizationServiceTests);
            var service = new RandomizationService();

            Assert.AreNotEqual(0, service.Next(key));
            Assert.IsTrue(service.Next(key, 10) <= 10);

            var valueBetween = service.Next(key, 0, 10);
            Assert.IsTrue(valueBetween >= 0 && valueBetween <= 10);
        }

        [TestMethod]
        public void NextBytes()
        {
            var buffer = new byte[255];
            const string key = nameof(RandomizationServiceTests);
            var service = new RandomizationService();

            service.NextBytes(key, buffer);
            Assert.IsTrue(buffer.Sum(o => o) > 0);
        }

        [TestMethod]
        public void NextDouble()
        {
            const string key = nameof(RandomizationServiceTests);
            var service = new RandomizationService();

            var result = service.NextDouble(key);
            Assert.IsTrue(result >= 0.0D);
        }
    }
}
