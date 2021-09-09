using GrillBot.Data.Models.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.Data.Models.Unverify
{
    [TestClass]
    public class UnverifyLogRemoveTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogRemove());
        }

        [TestMethod]
        public void FilledValues()
        {
            var log = new UnverifyLogRemove()
            {
                ReturnedOverwrites = new(),
                ReturnedRoles = new()
            };

            TestHelpers.CheckNonDefaultPropertyValues(log);
        }
    }
}
