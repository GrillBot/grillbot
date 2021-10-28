using GrillBot.App.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.App.Helpers
{
    [TestClass]
    public class CommonHelperTests
    {
        [TestMethod]
        public void Ok()
        {
            CommonHelper.SuppressException<InvalidOperationException>(() => Console.WriteLine("Test"));
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Exception()
        {
            CommonHelper.SuppressException<InvalidOperationException>(() => throw new InvalidOperationException());
            Assert.IsTrue(true);
        }
    }
}
