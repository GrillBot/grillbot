using GrillBot.App.Infrastructure.TypeReaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders
{
    [TestClass]
    public class DateTimeTypeReaderTests
    {
        [TestMethod]
        public void Read_Culture_True()
        {
            var datetime = new DateTime(2021, 12, 14, 10, 0, 0);
            var strDate = datetime.ToString("o", CultureInfo.InvariantCulture);

            var reader = new DateTimeTypeReader();
            var result = reader.ReadAsync(null, strDate, null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(datetime, result.Values.First().Value);
        }

        [TestMethod]
        public void Read_Regex_True()
        {
            var cases = new[] { "today", "tommorow", "yesterday", "pozajtra", "now" };

            var reader = new DateTimeTypeReader();
            foreach (var @case in cases)
            {
                var result = reader.ReadAsync(null, @case, null).Result;

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(1, result.Values.Count);
            }
        }

        [TestMethod]
        public void Read_Regex_False()
        {
            var reader = new DateTimeTypeReader();
            var result = reader.ReadAsync(null, "test", null).Result;

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
