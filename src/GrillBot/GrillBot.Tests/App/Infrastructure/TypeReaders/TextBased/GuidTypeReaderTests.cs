using GrillBot.App.Infrastructure.TypeReaders.TextBased;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GrillBot.Tests.App.Infrastructure.TypeReaders.TextBased
{
    [TestClass]
    public class GuidTypeReaderTests
    {
        [TestMethod]
        public void Read_Success()
        {
            var guid = Guid.NewGuid();

            var reader = new GuidTypeReader();
            var result = reader.ReadAsync(null, guid.ToString(), null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(guid, (Guid)result.Values.First().Value);
        }

        [TestMethod]
        public void Read_Fail()
        {
            var reader = new GuidTypeReader();
            var result = reader.ReadAsync(null, "ABCD", null).Result;

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
