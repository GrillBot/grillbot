using GrillBot.App.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class StringExtensionsTests
    {
        [TestMethod]
        public void Cut_Null()
        {
            var result = ((string)null).Cut(0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void Cut_Empty()
        {
            var result = "".Cut(100);

            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void Cut_Longer()
        {
            const string str = "Hello world";
            var result = str.Cut(5);

            Assert.AreEqual("He...", result);
        }

        [TestMethod]
        public void Cut_Shorter()
        {
            const string str = "Hello world";
            var result = str.Cut(50);

            Assert.AreEqual(str, result);
        }

        [TestMethod]
        public void Cut_WithoutDots()
        {
            const string str = "Hello world";
            var result = str.Cut(5, true);

            Assert.AreEqual("Hello", result);
        }
    }
}
