using GrillBot.Data.Models.API.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.API.Unverify
{
    [TestClass]
    public class UnverifyLogUpdateTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogUpdate());
        }
    }
}
