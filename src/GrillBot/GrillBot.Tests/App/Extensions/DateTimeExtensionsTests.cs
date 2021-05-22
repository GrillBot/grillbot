using GrillBot.App.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class DateTimeExtensionsTests
    {
        [TestMethod]
        public void ToCzechFormat_WithTime()
        {
            var datetime = new DateTime(2021, 5, 22, 19, 35, 34);
            const string expected = "22. 05. 2021 19:35:34";

            Assert.AreEqual(expected, datetime.ToCzechFormat());
        }

        [TestMethod]
        public void ToCzechFormat_WithoutTime()
        {
            var datetime = new DateTime(2021, 5, 22, 19, 35, 34);
            const string expected = "22. 05. 2021";

            Assert.AreEqual(expected, datetime.ToCzechFormat(true));
        }
    }
}
