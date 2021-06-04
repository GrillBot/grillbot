using GrillBot.Data.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace GrillBot.Tests.Data.Models
{
    [TestClass]
    public class AutoReplyConfigurationTests
    {
        [TestMethod]
        public void RegexOptions_CaseSensitive()
        {
            var configuration = new AutoReplyConfiguration()
            {
                CaseSensitive = true
            };

            const RegexOptions expected = RegexOptions.Multiline | RegexOptions.None;

            Assert.AreEqual(expected, configuration.Options);
        }

        [TestMethod]
        public void RegexOptions_CaseInsensitive()
        {
            var configuration = new AutoReplyConfiguration() { CaseSensitive = false };
            const RegexOptions expected = RegexOptions.Multiline | RegexOptions.IgnoreCase;

            Assert.AreEqual(expected, configuration.Options);
        }
    }
}
