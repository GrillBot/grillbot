using GrillBot.Data.Models.MathJS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.MathJS
{
    [TestClass]
    public class MathJSRequestTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new MathJSRequest());
        }

        [TestMethod]
        public void FilledValues()
        {
            var metadata = new MathJSRequest()
            {
                Expression = "Expr"
            };

            TestHelpers.CheckNonDefaultPropertyValues(metadata);
        }
    }
}
