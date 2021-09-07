using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Database.Entity
{
    [TestClass]
    public class EmoteStatisticItemTests
    {
        [TestMethod]
        public void Entity_Properties_Default()
        {
            TestHelpers.CheckDefaultPropertyValues(new EmoteStatisticItem(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void Entity_Properties_Filled()
        {
            var item = new EmoteStatisticItem()
            {
                EmoteId = "Emote",
                FirstOccurence = DateTime.MaxValue,
                LastOccurence = DateTime.MaxValue,
                UseCount = 50,
                User = new(),
                UserId = "User"
            };

            TestHelpers.CheckDefaultPropertyValues(item, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
