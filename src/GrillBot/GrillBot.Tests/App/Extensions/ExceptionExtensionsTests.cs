using GrillBot.App.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;

namespace GrillBot.Tests.App.Extensions
{
    [TestClass]
    public class ExceptionExtensionsTests
    {
        [TestMethod]
        public void ToMemoryStream()
        {
            var exception = new Exception("Testing");
            using var memoryStream = exception.ToMemoryStream();

            var content = Encoding.UTF8.GetString(memoryStream.ToArray());
            Assert.AreEqual(exception.ToString(), content);
        }
    }
}
