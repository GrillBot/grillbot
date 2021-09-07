using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class RemindMessageTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new RemindMessage(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var remind = new RemindMessage()
            {
                Id = 42,
                At = DateTime.MaxValue,
                FromUser = new(),
                FromUserId = "ABCD",
                Message = "ABCD",
                OriginalMessageId = "ABCD",
                Postpone = 50,
                RemindMessageId = "ABCD",
                ToUser = new(),
                ToUserId = "ABCD"
            };

            TestHelpers.CheckDefaultPropertyValues(remind, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
