using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.TextBased;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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

            var context = new Mock<ICommandContext>().Object;
            var reader = new GuidTypeReader();
            var result = reader.ReadAsync(context, guid.ToString(), null).Result;

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(guid, (Guid)result.Values.First().Value);
        }

        [TestMethod]
        public void Read_Fail()
        {
            var context = new Mock<ICommandContext>().Object;
            var reader = new GuidTypeReader();
            var result = reader.ReadAsync(context, "ABCD", null).Result;

            Assert.IsFalse(result.IsSuccess);
        }
    }
}
