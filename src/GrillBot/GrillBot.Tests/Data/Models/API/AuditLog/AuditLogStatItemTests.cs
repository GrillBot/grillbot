using GrillBot.Data.Models.API.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.AuditLog
{
    [TestClass]
    public class AuditLogStatItemTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new AuditLogStatItem());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var item = new AuditLogStatItem("Test", 50, DateTime.MinValue, DateTime.MaxValue);
            TestHelpers.CheckNonDefaultPropertyValues(item);
        }

        [TestMethod]
        public void FilledConstructor_NullCount()
        {
            var item = new AuditLogStatItem("Test", null, DateTime.MinValue, DateTime.MaxValue);
            TestHelpers.CheckDefaultPropertyValues(item, (defaultValue, value, propertyName) =>
            {
                if (propertyName == "Count") Assert.AreEqual(defaultValue, value);
                else Assert.AreNotEqual(defaultValue, value);
            });
        }
    }
}
