using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.TextBased;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Globalization;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders.TextBased
{
    [TestClass]
    public class DateTimeTypeReaderTests
    {
        [TestMethod]
        public void Read_Culture_True()
        {
            var datetime = new DateTime(2021, 12, 14, 10, 0, 0);
            var strDate = datetime.ToString("o", CultureInfo.InvariantCulture);

            var context = new Mock<ICommandContext>().Object;
            var reader = new DateTimeTypeReader();
            var result = reader.ReadAsync(context, strDate, null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(datetime, result.Values.First().Value);
        }

        [TestMethod]
        public void Read_Regex_True()
        {
            var cases = new[] { "today", "tommorow", "yesterday", "pozajtra", "now" };

            var context = new Mock<ICommandContext>().Object;
            var reader = new DateTimeTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(context, @case, null).Result;

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(1, result.Values.Count);
            }
        }

        [TestMethod]
        public void Read_Regex_False()
        {
            var context = new Mock<ICommandContext>().Object;
            var reader = new DateTimeTypeReader();
            var result = reader.ReadAsync(context, "test", null).Result;

            Assert.IsFalse(result.IsSuccess);
        }

        [TestMethod]
        public void Read_EN()
        {
            var context = new Mock<ICommandContext>().Object;
            var reader = new DateTimeTypeReader();
            var result = reader.ReadAsync(context, "07/02/2021", null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(new DateTime(2021, 07, 02), result.Values.First().Value);
        }

        [TestMethod]
        public void Read_TimeShift()
        {
            var cases = new[] { "1m", "1h", "1d", "1M", "1r", "1y", "1h1m", "3d4h5m" };

            var context = new Mock<ICommandContext>().Object;
            var reader = new DateTimeTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(context, @case, null).Result;

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(1, result.Values.Count);
                Assert.IsTrue(result.Values.First().Value is DateTime);
            }
        }
    }
}
