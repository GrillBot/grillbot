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

        [TestMethod]
        public void FormatMessagesToCzech_One()
        {
            var result = FormatHelper.FormatMessagesToCzech(1);

            Assert.AreEqual("1 zpráva", result);
        }

        [TestMethod]
        public void FormatMessagesToCzech_TwoToFour()
        {
            var result = FormatHelper.FormatMessagesToCzech(3);

            Assert.AreEqual("3 zprávy", result);
        }

        [TestMethod]
        public void FormatMessagesToCzech_MoreThanFour()
        {
            var result = FormatHelper.FormatMessagesToCzech(10);

            Assert.AreEqual("10 zpráv", result);
        }

        [TestMethod]
        public void FormatMessagesToCzech_Zero()
        {
            var result = FormatHelper.FormatMessagesToCzech(0);

            Assert.AreEqual("0 zpráv", result);
        }

        [TestMethod]
        public void FormatPermissionsToCzech()
        {
            Assert.AreEqual("1 oprávnění", FormatHelper.FormatPermissionstoCzech(1));
        }

        [TestMethod]
        public void FormatPointsToCzech()
        {
            Assert.AreEqual("1 bod", FormatHelper.FormatPointsToCzech(1));
        }
    }
}
