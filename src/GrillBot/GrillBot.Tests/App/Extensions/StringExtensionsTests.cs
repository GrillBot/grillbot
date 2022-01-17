using GrillBot.Data.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

        [TestMethod]
        public void ParseTime_Success_Seconds()
        {
            var timeSpan = "13:35:26".ParseTime();
            Assert.IsNotNull(timeSpan);
            Assert.AreEqual(new TimeSpan(13, 35, 26), timeSpan);
        }

        [TestMethod]
        public void ParseTime_Success_WithoutSeconds()
        {
            var timeSpan = "13:35".ParseTime();
            Assert.IsNotNull(timeSpan);
            Assert.AreEqual(new TimeSpan(13, 35, 0), timeSpan);
        }

        [TestMethod]
        public void ParseTime_Success_FromAny()
        {
            var timeSpan = "Cas 13:35".ParseTime(true);
            Assert.IsNotNull(timeSpan);
            Assert.AreEqual(new TimeSpan(13, 35, 0), timeSpan);
        }

        [TestMethod]
        public void ParseTime_Error()
        {
            var timeSpan = "Cas".ParseTime();
            Assert.IsNull(timeSpan);
        }
    }
}
