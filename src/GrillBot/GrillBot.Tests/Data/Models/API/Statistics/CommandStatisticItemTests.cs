using GrillBot.Data.Models.API.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Statistics
{
    [TestClass]
    public class CommandStatisticItemTests
    {
        [TestMethod]
        public void DefaultValues()
        {
            TestHelpers.CheckDefaultPropertyValues(new CommandStatisticItem());
        }

        [TestMethod]
        public void SuccessRates()
        {
            var cases = new[]
            {
                new Tuple<int, CommandStatisticItem>(0, new CommandStatisticItem() { SuccessCount = 0, FailedCount = 0 }),
                new Tuple<int, CommandStatisticItem>(100, new CommandStatisticItem() { SuccessCount = 1, FailedCount = 0 }),
                new Tuple<int, CommandStatisticItem>(50, new CommandStatisticItem() { SuccessCount = 1, FailedCount = 1 }),
            };

            foreach (var @case in cases)
            {
                var rate = @case.Item2.SuccessRate;
                Assert.AreEqual(@case.Item1, rate);
            }
        }

        [TestMethod]
        public void NonDefaultValues()
        {
            var item = new CommandStatisticItem()
            {
                Command = "Command",
                FailedCount = 50,
                LastCall = DateTime.MaxValue,
                SuccessCount = 50
            };

            TestHelpers.CheckNonDefaultPropertyValues(item);
        }
    }
}
