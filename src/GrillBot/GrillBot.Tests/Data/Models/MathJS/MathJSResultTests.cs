using GrillBot.Data.Models.MathJS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.MathJS
{
    [TestClass]
    public class MathJSResultTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new MathJSResult(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void FilledValues()
        {
            var metadata = new MathJSResult()
            {
                Error = "Err",
                Result = "Res"
            };

            TestHelpers.CheckDefaultPropertyValues(metadata, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
