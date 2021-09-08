using GrillBot.Data.Models.API.Reminder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Reminder
{
    [TestClass]
    public class RemindMessageTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new RemindMessage());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var entity = new GrillBot.Database.Entity.RemindMessage()
            {
                FromUser = new(),
                ToUser = new(),
                Id = 50,
                At = DateTime.MaxValue,
                Message = "Message",
                Postpone = 100
            };

            TestHelpers.CheckNonDefaultPropertyValues(new RemindMessage(entity));
        }
    }
}
