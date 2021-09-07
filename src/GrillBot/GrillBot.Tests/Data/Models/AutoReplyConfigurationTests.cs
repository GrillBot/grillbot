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

        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new AutoReplyConfiguration(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "Options":
                        break;
                    default:
                        Assert.AreEqual(defaultValue, value);
                        break;
                }
            });
        }

        [TestMethod]
        public void FilledValues()
        {
            var configuration = new AutoReplyConfiguration()
            {
                CaseSensitive = true,
                Disabled = true,
                Reply = "Reply",
                Template = "Template"
            };

            TestHelpers.CheckDefaultPropertyValues(configuration, (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "Options":
                        break;
                    default:
                        Assert.AreNotEqual(defaultValue, value);
                        break;
                }
            });
        }
    }
}
