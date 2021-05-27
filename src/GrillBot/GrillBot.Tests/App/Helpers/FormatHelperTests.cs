using GrillBot.App.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Helpers
{
    [TestClass]
    public class FormatHelperTests
    {
        [TestMethod]
        public void FormatMembersToCzech_One()
        {
            var result = FormatHelper.FormatMembersToCzech(1);

            Assert.AreEqual("1 člen", result);
        }

        [TestMethod]
        public void FormatMembersToCzech_TwoToFour()
        {
            var result = FormatHelper.FormatMembersToCzech(3);

            Assert.AreEqual("3 členové", result);
        }

        [TestMethod]
        public void FormatMembersToCzech_MoreThanFour()
        {
            var result = FormatHelper.FormatMembersToCzech(10);

            Assert.AreEqual("10 členů", result);
        }

        [TestMethod]
        public void FormatMembersToCzech_Zero()
        {
            var result = FormatHelper.FormatMembersToCzech(0);

            Assert.AreEqual("0 členů", result);
        }

        [TestMethod]
        public void FormatBooleanToCzech_True()
        {
            var result = FormatHelper.FormatBooleanToCzech(true);

            Assert.AreEqual("Ano", result);
        }

        [TestMethod]
        public void FormatBooleanToCzech_False()
        {
            var result = FormatHelper.FormatBooleanToCzech(false);

            Assert.AreEqual("Ne", result);
        }
    }
}
