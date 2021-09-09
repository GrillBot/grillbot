using GrillBot.Data.Models.Unverify;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.Unverify
{
    [TestClass]
    public class UnverifyLogUpdateTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new UnverifyLogUpdate());
        }

        [TestMethod]
        public void FilledValues()
        {
            var log = new UnverifyLogUpdate()
            {
                End = DateTime.MaxValue,
                Start = DateTime.MaxValue
            };

            TestHelpers.CheckNonDefaultPropertyValues(log);
        }
    }
}
