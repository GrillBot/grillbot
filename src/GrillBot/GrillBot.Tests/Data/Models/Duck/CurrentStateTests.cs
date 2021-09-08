using GrillBot.Data.Enums;
using GrillBot.Data.Models.Duck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.Duck
{
    [TestClass]
    public class CurrentStateTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new CurrentState(), (defaultValue, value, _) => Assert.AreEqual(defaultValue, value));
        }

        [TestMethod]
        public void FilledValues()
        {
            var state = new CurrentState()
            {
                BeersOnTap = new[] { "" },
                EventName = "Event",
                ExpectedEnd = DateTime.Now,
                LastChange = DateTime.Now,
                NextOpeningDateTime = DateTime.Now,
                NextPlannedState = DuckState.OpenBar,
                NextStateDateTime = DateTime.Now,
                Note = "Note",
                OpenedByDiscordGlobalNick = "Nick",
                OpenedByName = "Name",
                State = DuckState.OpenBar
            };

            TestHelpers.CheckDefaultPropertyValues(state, (defaultValue, value, _) => Assert.AreNotEqual(defaultValue, value));
        }
    }
}
