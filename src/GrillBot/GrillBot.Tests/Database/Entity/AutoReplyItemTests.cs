using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class AutoReplyItemTests
    {
        [TestMethod]
        public void EmptyData()
        {
            TestHelpers.CheckDefaultPropertyValues(new AutoReplyItem(), (defaultValue, value, propertyName) =>
            {
                switch (propertyName)
                {
                    case "RegexOptions":
                        Assert.AreEqual(RegexOptions.Multiline | RegexOptions.IgnoreCase, value);
                        break;
                    default:
                        Assert.AreEqual(defaultValue, value);
                        break;
                }
            });
        }

        [TestMethod]
        public void FilledData()
        {
            var item = new AutoReplyItem()
            {
                Flags = 3,
                Id = 1,
                Reply = "Test",
                Template = "Test"
            };

            TestHelpers.CheckNonDefaultPropertyValues(item);
        }
    }
}
